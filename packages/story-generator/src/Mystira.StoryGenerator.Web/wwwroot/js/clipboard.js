(function () {
    const fallbackCopy = (text) => {
        try {
            const textarea = document.createElement('textarea');
            textarea.value = text ?? '';
            textarea.setAttribute('readonly', '');
            textarea.style.position = 'absolute';
            textarea.style.left = '-9999px';
            document.body.appendChild(textarea);
            textarea.select();
            textarea.setSelectionRange(0, textarea.value.length);
            const successful = document.execCommand('copy');
            document.body.removeChild(textarea);
            return successful;
        } catch (err) {
            console.warn('Mystira clipboard fallback failed', err);
            return false;
        }
    };

    const copyText = async (text) => {
        const payload = typeof text === 'string' ? text : (text ?? '').toString();
        if (!payload) {
            return false;
        }

        if (navigator.clipboard && typeof navigator.clipboard.writeText === 'function') {
            try {
                await navigator.clipboard.writeText(payload);
                return true;
            } catch (err) {
                console.warn('Mystira clipboard writeText failed, attempting fallback', err);
                return fallbackCopy(payload);
            }
        }

        return fallbackCopy(payload);
    };

    const readText = async () => {
        try {
            if (navigator.clipboard && typeof navigator.clipboard.readText === 'function') {
                const text = await navigator.clipboard.readText();
                return { success: true, text: text };
            }
        } catch (err) {
            console.warn('Mystira clipboard readText failed', err);
        }
        return { success: false, text: null, error: 'Clipboard read not available' };
    };

    window.mystiraClipboard = window.mystiraClipboard || {};
    window.mystiraClipboard.copyText = copyText;
    window.mystiraClipboard.readText = readText;
})();
