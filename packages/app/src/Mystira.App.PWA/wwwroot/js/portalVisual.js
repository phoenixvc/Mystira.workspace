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

        /**
         * Fetch an SVG file and inject its markup into a container element,
         * so internal class names are part of the DOM and CSS animations apply.
         * Prefixes all id/url(#...) references with a unique prefix to avoid collisions.
         */
        injectFrameSvg: async function (containerElement, svgUrl, idPrefix) {
            if (!containerElement || !svgUrl) return;
            try {
                var response = await fetch(svgUrl);
                if (!response.ok) {
                    console.warn("MystiraPortal: failed to load SVG frame from", svgUrl, "status:", response.status);
                    return;
                }
                var svgText = await response.text();
                // Prefix all id="..." and url(#...) references to avoid SVG ID collisions
                if (idPrefix) {
                    svgText = svgText.replace(/id="([^"]+)"/g, 'id="' + idPrefix + '-$1"');
                    svgText = svgText.replace(/url\(#([^)]+)\)/g, 'url(#' + idPrefix + '-$1)');
                }
                containerElement.innerHTML = svgText;
                // Apply the frame class to the injected <svg> element
                var svg = containerElement.querySelector("svg");
                if (svg) {
                    svg.classList.add("ornate-frame-svg");
                    svg.setAttribute("preserveAspectRatio", "xMidYMid meet");
                    svg.setAttribute("aria-hidden", "true");
                }
            } catch (err) {
                console.warn("MystiraPortal: error injecting SVG frame:", err);
            }
        },

        // Stores { selector, el, onMove, onLeave } for cleanup
        _parallaxBindings: [],

        bindParallax: function (selector) {
            var el = document.querySelector(selector);
            if (!el) return;

            function onMove(e) {
                var rect = el.getBoundingClientRect();
                var x = (e.clientX - rect.left) / rect.width;
                var y = (e.clientY - rect.top) / rect.height;
                el.style.setProperty("--pointer-x", x.toFixed(3));
                el.style.setProperty("--pointer-y", y.toFixed(3));
            }

            function onLeave() {
                el.style.setProperty("--pointer-x", "0.5");
                el.style.setProperty("--pointer-y", "0.5");
            }

            el.addEventListener("mousemove", onMove);
            el.addEventListener("mouseleave", onLeave);

            this._parallaxBindings.push({ selector: selector, el: el, onMove: onMove, onLeave: onLeave });
        },

        unbindParallax: function (selector) {
            for (var i = this._parallaxBindings.length - 1; i >= 0; i--) {
                var b = this._parallaxBindings[i];
                if (b.selector === selector) {
                    b.el.removeEventListener("mousemove", b.onMove);
                    b.el.removeEventListener("mouseleave", b.onLeave);
                    this._parallaxBindings.splice(i, 1);
                }
            }
        }
    };
})();
