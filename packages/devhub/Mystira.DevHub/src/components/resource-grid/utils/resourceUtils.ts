export function getStatusColor(status: string): string {
  switch (status) {
    case "running":
      return "text-green-700 bg-green-100";
    case "stopped":
      return "text-gray-700 bg-gray-100";
    case "warning":
      return "text-yellow-700 bg-yellow-100";
    case "failed":
      return "text-red-700 bg-red-100";
    default:
      return "text-gray-700 bg-gray-100";
  }
}

export function getStatusIcon(status: string): string {
  switch (status) {
    case "running":
      return "✅";
    case "stopped":
      return "⏸️";
    case "warning":
      return "⚠️";
    case "failed":
      return "❌";
    default:
      return "❓";
  }
}

export function getResourceIcon(type: string): string {
  if (type.includes("cosmos") || type.includes("database")) return "🗄️";
  if (type.includes("storage")) return "📦";
  if (type.includes("app") || type.includes("site")) return "🌐";
  if (type.includes("analytics")) return "📊";
  if (type.includes("insights")) return "📈";
  if (type.includes("communication")) return "💬";
  return "📋";
}

export function formatCost(cost: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    minimumFractionDigits: 2,
  }).format(cost);
}

export function openInPortal(resourceId: string): void {
  const portalUrl = `https://portal.azure.com/#@/resource${resourceId}`;
  console.log("Open in portal:", portalUrl);
  alert(`Would open Azure Portal for resource: ${resourceId}`);
}
