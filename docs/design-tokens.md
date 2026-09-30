# Design Tokens

Blue-and-white, minimal — one accent color, flat hairline borders instead of
drop shadows, restrained type scale. Defined in
`MedConnect/wwwroot/css/tokens.css` and loaded ahead of `app.css`.

| Token | Value | Use |
|---|---|---|
| `--bg` | `#ffffff` | page background |
| `--bg-subtle` | `#f5f8fc` | sidebar / card backgrounds |
| `--border` | `#e2e8f0` | hairlines |
| `--text` | `#0f172a` | primary text |
| `--text-muted` | `#64748b` | secondary text |
| `--accent` | `#2563eb` | links, primary buttons, active nav, focus rings |
| `--accent-hover` | `#1d4ed8` | hover state |
| `--accent-subtle` | `#eff6ff` | selected-row / active-pill backgrounds |
| `--radius` | `10px` | consistent corner radius everywhere |
| `--shadow-float` | `0 4px 16px rgba(15,23,42,0.08)` | reserved for floating elements (modals, dropdowns) only |
| `--font-sans` | `-apple-system, "SF Pro Text", Inter, system-ui, sans-serif` | all text |

Type scale: `--text-xs` 13px, `--text-sm` 15px, `--text-base` 17px,
`--text-lg` 22px, `--text-xl` 28px.

Spacing scale: `--space-1` 4px through `--space-5` 32px.

## Layout conventions

- Fixed sidebar nav (icon + label, subtle pill highlight on the active route),
  macOS-app style rather than a top navbar.
- Content in cards with generous whitespace, not dense Bootstrap-table rows.
- Nothing but `--accent` gets color except status badges (referral status,
  appointment status).
