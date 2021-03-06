export default function getWorkflowApi() {
  if (process.env.NODE_ENV !== 'production') {
    return `${process.env.PUBLIC_URL}/data/mock-workflows.json`;
  }
  return '/sitecore/api/ssc/workbox/workflows';
}

export function getWorkflowDetails() {
  if (process.env.NODE_ENV !== 'production') {
    return `${process.env.PUBLIC_URL}/data/mock-workflow-details.json`;
  }
  return '/sitecore/api/ssc/workbox/detail/{workflowid}';
}

