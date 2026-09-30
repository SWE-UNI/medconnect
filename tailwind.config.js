module.exports = {
    // Class tokens are static literals inside Razor components (markup and
    // @code string constants) plus C# in the client project. CSS files are
    // hand-built and are NOT scanned (no @apply/tailwind directives there).
    content: [
        './MedConnect.Client/**/*.razor',
        './MedConnect.Client/**/*.cs',
        './MedConnect.Client/wwwroot/index.html',
    ],
    // Mirrors the old runtime config: the app ships its own hand-built design
    // system (tokens.css/app.css) as the base layer, so Tailwind's reset is
    // disabled to avoid clobbering it.
    corePlugins: {
        preflight: false,
    },
};