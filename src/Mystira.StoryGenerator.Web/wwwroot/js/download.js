window.downloadFile = function(fileName, content, mimeType) {
  try {
    let blobContent = content;

    // If content is a string, attempt to decode as base64 first; if that fails, treat as plain text
    if (typeof content === 'string') {
      try {
        // Remove any data URL prefix if present
        const base64 = content.includes(',') && content.startsWith('data:')
          ? content.substring(content.indexOf(',') + 1)
          : content;

        const binaryString = atob(base64);
        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
          bytes[i] = binaryString.charCodeAt(i);
        }
        blobContent = bytes; // Successfully decoded base64
      } catch {
        // Not valid base64; keep as plain text
        blobContent = content;
      }
    }

    const mimeTypeToUse = mimeType || 'text/plain;charset=utf-8';
    const blob = new Blob([blobContent], { type: mimeTypeToUse });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName || 'download.txt';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  } catch (e) {
    console.error('Download failed', e);
  }
};

// Download helper for plain text content (no base64). Useful for CSV exports.
window.downloadFileText = function(fileName, textContent, mimeType) {
  try {
    const type = mimeType || 'text/plain;charset=utf-8';
    const blob = new Blob([textContent], { type });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName || 'download.txt';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  } catch (e) {
    console.error('Download (text) failed', e);
  }
};
