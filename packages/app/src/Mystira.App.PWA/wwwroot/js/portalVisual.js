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
        },

        bindParallax: function (selector) {
            var el = document.querySelector(selector);
            if (!el) return;
            el.addEventListener("mousemove", function (e) {
                var rect = el.getBoundingClientRect();
                var x = (e.clientX - rect.left) / rect.width;
                var y = (e.clientY - rect.top) / rect.height;
                el.style.setProperty("--pointer-x", x.toFixed(3));
                el.style.setProperty("--pointer-y", y.toFixed(3));
            });
            el.addEventListener("mouseleave", function () {
                el.style.setProperty("--pointer-x", "0.5");
                el.style.setProperty("--pointer-y", "0.5");
            });
        }
    };
})();
