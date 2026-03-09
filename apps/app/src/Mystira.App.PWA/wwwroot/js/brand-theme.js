window.mystiraTheme = {
  init: function () {
    const saved = localStorage.getItem("mystira-theme");
    const isDark = saved ? saved === "dark" : false;
    document.documentElement.setAttribute(
      "data-theme",
      isDark ? "dark" : "light"
    );
    return isDark;
  },
  toggle: function () {
    const current =
      document.documentElement.getAttribute("data-theme") || "light";
    const next = current === "dark" ? "light" : "dark";
    document.documentElement.setAttribute("data-theme", next);
    localStorage.setItem("mystira-theme", next);
    return next === "dark";
  },
};
