/**
 * Portal Visual — minimal JS interop for video end detection
 */
(function () {
    "use strict";

    window.MystiraPortal = {
        bindVideoEnd: function (videoElement, dotNetRef) {
            if (!videoElement) return;
            videoElement.addEventListener("ended", function () {
                dotNetRef.invokeMethodAsync("OnVideoEnded");
            }, { once: true });
        }
    };
})();
