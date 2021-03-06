﻿using Feature.Workbox.Interfaces;
using Feature.Workbox.Models;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System.Collections.Generic;
using System.Linq;

namespace Feature.Workbox.Services
{
    public class WorkflowRepository : IWorkflowRepository
    {
        public DetailedWorkflow GetDetailedWorkflow(string id)
        {
            var response = new DetailedWorkflow();
            var wfItem = Sitecore.Context.Database.GetItem(new ID(id));

            response.Id = wfItem.ID.ToString();
            response.Name = wfItem.Name;

            List<Item> items = new List<Item>();
            // TODO: REWRITE TO CONTENT SEARCH
            //"__Workflow", "__Workflow state"
            using (new SecurityDisabler())
            {
                using (new DatabaseSwitcher(Factory.GetDatabase(Constants.Databases.Master)))
                {
                    items = Sitecore.Context.Database.GetItem(new ID("{0DE95AE4-41AB-4D01-9EB0-67441B7C2450}")).Axes.GetDescendants().ToList();
                }
            }

            foreach (var wfState in wfItem.Children.Where(t => t.TemplateID == Constants.WorkflowState.TemplateId))
            {
                var state = new DetailedWorkflowState
                {
                    Id = wfState.ID.ToString(),
                    IsFinal = wfState[Constants.WorkflowState.Fields.Final] == "1",
                    Name = wfState.Name
                };

               
               
                state.Items = items.Where(t => t["__Workflow"].Equals(wfItem.ID.ToString()) && t["__Workflow State"].Equals(wfState.ID.ToString()))
                    .Select(testc => new WorkflowItem
                    {
                        ID = testc.ID.ToString(),
                        Name = testc.Name,
                        NextStates = GetNextStates(testc, wfState)
                    }).ToList();

                

                response.States.Add(state);
            }

            return response;
        }

        public List<Workflow> GetWorkflows()
        {
            var result = new List<Workflow>();
            int i = 0;
            using (new SecurityDisabler())
            {
                using (new DatabaseSwitcher(Factory.GetDatabase(Constants.Databases.Master)))
                {
                    var wfRootItem = Sitecore.Context.Database.GetItem(new ID(Constants.ItemIds.WorkflowRootFolderId));

                    foreach (var wfItem in wfRootItem.Children.Where(t => t.TemplateID == Constants.Workflow.TemplateId))
                    {
                        var workflow = new Workflow
                        {
                            Id = wfItem.ID.ToString(),
                            Name = wfItem.Name,
                            IsSelected = i == 0 // TODO: Change it
                        };

                        foreach (var wfState in wfItem.Children.Where(t => t.TemplateID == Constants.WorkflowState.TemplateId))
                        {
                            var state = new WorkflowState
                            {
                                Id = wfState.ID.ToString(),
                                IsFinal = wfState[Constants.WorkflowState.Fields.Final] == "1",
                                Name = wfState.Name
                            };

                            workflow.States.Add(state);
                        }

                        result.Add(workflow);
                        i++;
                    }
                }
            }

            return result;
        }

        private List<NextWorkflowState> GetNextStates(Item item, Item wfStateItem)
        {
            var result = new List<NextWorkflowState>();
            foreach(var wfCommand in wfStateItem.Children.Where(t => t.TemplateID == Constants.WorkflowCommand.TemplateId))
            {

                var nextState = result.FirstOrDefault(testc => testc.Id == wfCommand[Constants.WorkflowCommand.Fields.NextState]);

                if (nextState == null)
                {
                    nextState = new NextWorkflowState
                    {
                        Id = wfCommand[Constants.WorkflowCommand.Fields.NextState],
                        Name = Sitecore.Context.Database.GetItem(new ID(wfCommand[Constants.WorkflowCommand.Fields.NextState])).Name
                    };

                    result.Add(nextState);
                }


                nextState.Actions.Add(new WorkboxAction
                {
                    ID = wfCommand.ID.ToString(),
                    Name = wfCommand.Name,
                    SuppressComment = wfCommand[Constants.WorkflowCommand.Fields.SuppressComment] == "1"
                });

            }
            return result;
        }

        private List<WorkboxAction> GetActions(Item item, List<Item> commands)
        {
            var result = new List<WorkboxAction>();
            // TODO:  //Appearance Evaluator Type EVALUATE
            foreach (var wfCommand in commands)
            {
                result.Add(new WorkboxAction
                {
                    ID = wfCommand.ID.ToString(),
                    Name = wfCommand.Name,
                    SuppressComment = wfCommand[Constants.WorkflowCommand.Fields.SuppressComment] == "1"
                });
            }
            return result;
        }

    }
}
