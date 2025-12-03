window.downloadFile = function(fileName, content, mimeType) {
  try {
    // Determine if content is base64 or raw
    const isBase64 = mimeType && (mimeType.includes('csv') || mimeType.includes('json'));
    
    let blobContent;
    if (isBase64) {
      // Convert base64 to binary
      const binaryString = atob(content);
      const bytes = new Uint8Array(binaryString.length);
      for (let i = 0; i < binaryString.length; i++) {
        bytes[i] = binaryString.charCodeAt(i);
      }
      blobContent = bytes;
    } else {
      // Raw content
      blobContent = content;
    }
    
    const mimeTypeToUse = mimeType || 'text/yaml;charset=utf-8';
    const blob = new Blob([blobContent], { type: mimeTypeToUse });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName || 'story.yaml';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  } catch (e) {
    console.error('Download failed', e);
  }
};
