/**
 * Portal Visual — minimal JS interop for video end detection
 */
(function () {
    "use strict";

    window.MystiraPortal = {
        bindVideoEnd: function (videoElement, dotNetRef) {
            if (!videoElement) return;

            // Guard: if the video already finished (e.g. cached + autoplay beat the JS binding),
            // fire the callback immediately instead of leaving the user stuck on the video.
            if (videoElement.ended) {
                dotNetRef.invokeMethodAsync("OnVideoEnded");
                return;
            }

            videoElement.addEventListener("ended", function () {
                dotNetRef.invokeMethodAsync("OnVideoEnded");
            }, { once: true });
        }
    };
})();
