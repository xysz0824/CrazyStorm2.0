import os
import json
from html import escape

project = "CrazyStorm 2.0 Tutorial"
version = "v2.0"
release = version
copyright = " Copyright 2026, StarX"
author = "CrazyStorm Contributors"

extensions = []
templates_path = ["_shared/_templates"]
exclude_patterns = ["_build", "Thumbs.db", ".DS_Store"]

language = "zh_CN"
html_search_language = "zh"

html_theme = "alabaster"
html_static_path = ["_shared/_static"]
html_logo = "_shared/_static/icon.ico"
html_css_files = ["custom.css"]
html_js_files = ["nav.js"]
html_show_search_summary = False
html_show_sourcelink = False
html_theme_options = {
    "fixed_sidebar": True,
    "show_powered_by": False,
    "page_width": "auto",
    "sidebar_width": "320px",
}
html_sidebars = {
    "**": [
        "about.html",
        "searchbox.html",
        "navigation.html",
    ]
}

# Support old and new Sphinx defaults for master document.
master_doc = "index"

# Language folder -> display name mapping for body-top language links.
cs_language_label_map = {
    "zh-cn": "简体中文",
    "en": "EN",
}

# Language folder -> display name mapping for root index page links.
cs_root_language_label_map = {
    "zh-cn": "简体中文",
    "en": "English",
}
cs_ui_default_language = "en"
cs_root_index_default_language = "en"

# Shared UI text catalog used by templates and nav.js.
cs_ui_text_catalog = {
    "en": {
        "home_label": "Home",
        "breadcrumb_label": "Breadcrumb",
        "language_switcher_label": "Language switcher",
        "page_nav_label": "Page navigation",
        "page_nav_prev": "Previous",
        "page_nav_next": "Next",
        "search_submit": "Search",
        "search_submit_rewrite_from": ["go", "search", "submit", "提交", "搜索"],
        "search_results_title": "Search Results",
        "search_no_match": "No matching documents were found.",
        "search_finished": "Search finished, found {count} results.",
        "search_tokens_preparing": ["preparing search", "正在准备搜索"],
        "search_tokens_searching": ["searching", "搜索中", "正在搜索", "正在搜索中"],
        "search_tokens_completed": ["search results", "搜索结果"],
        "nav_toggle_aria_label": "Toggle submenu",
        "mobile_nav_open": "Open navigation",
        "mobile_nav_close": "Close navigation",
        "mobile_nav_label": "Mobile navigation",
        "mobile_nav_menu": "Menu",
    },
    "zh-cn": {
        "home_label": "首页",
        "breadcrumb_label": "面包屑导航",
        "language_switcher_label": "语言切换",
        "page_nav_label": "页面导航",
        "page_nav_prev": "上一页",
        "page_nav_next": "下一页",
        "search_submit": "搜索",
        "search_submit_rewrite_from": ["go", "search", "submit", "提交", "搜索"],
        "search_results_title": "搜索结果",
        "search_no_match": "没有找到匹配内容。",
        "search_finished": "检索完成，找到 {count} 个结果。",
        "search_tokens_preparing": ["preparing search", "正在准备搜索"],
        "search_tokens_searching": ["searching", "搜索中", "正在搜索", "正在搜索中"],
        "search_tokens_completed": ["search results", "搜索结果"],
        "nav_toggle_aria_label": "切换子菜单",
        "mobile_nav_open": "打开导航",
        "mobile_nav_close": "关闭导航",
        "mobile_nav_label": "移动端导航",
        "mobile_nav_menu": "目录",
    },
}

# Root entry page text catalog used by generated Docs/index.html.
cs_root_index_text_catalog = {
    "en": {
        "title": "CrazyStorm 2.0 Tutorial Entry",
        "heading": "CrazyStorm 2.0 Tutorial",
        "tip": "This is the local offline tutorial entry page. In the application, Help -> Tutorial will prefer the matching system language automatically.",
        "choose_language": "Choose Language",
        "empty": "No language docs are currently built. Please run Docs/build.ps1 first.",
        "html_lang": "en",
    },
    "zh-cn": {
        "title": "CrazyStorm 2.0 教程入口",
        "heading": "CrazyStorm 2.0 教程",
        "tip": "这是本地离线教程入口页。程序内“帮助 -> 教程”会按系统语言自动优先打开对应语言版本。",
        "choose_language": "选择语言",
        "empty": "暂无已构建的语言文档，请先运行 Docs/build.ps1。",
        "html_lang": "zh-CN",
    },
}

_cs_language_codes = []
_cs_language_labels = {}
_cs_language_docs = {}
_cs_generated_docnames = {"search", "genindex"}


def _normalize_language_code(value):
    if not value:
        return ""
    return str(value).strip().lower().replace("_", "-")


def _normalize_docname(value):
    if not value:
        return "index"

    text = str(value).strip().replace("\\", "/").strip("/")
    if not text:
        return "index"
    if text.endswith(".rst"):
        text = text[:-4]
    elif text.endswith(".html"):
        text = text[:-5]
    return text


def _discover_language_sources(source_root):
    language_sources = {}
    if not os.path.isdir(source_root):
        return language_sources

    for entry_name in os.listdir(source_root):
        if not entry_name or entry_name.startswith("_") or entry_name.startswith("."):
            continue

        entry_path = os.path.join(source_root, entry_name)
        if not os.path.isdir(entry_path):
            continue
        if not os.path.isfile(os.path.join(entry_path, "index.rst")):
            continue

        code = _normalize_language_code(entry_name)
        if code and code not in language_sources:
            language_sources[code] = entry_path
    return language_sources


def _collect_docnames(language_source):
    docnames = set()
    for root, _, files in os.walk(language_source):
        for file_name in files:
            if not file_name.endswith(".rst"):
                continue

            full_path = os.path.join(root, file_name)
            relative_path = os.path.relpath(full_path, language_source)
            docname = _normalize_docname(relative_path)
            if docname:
                docnames.add(docname)

    docnames.update(_cs_generated_docnames)
    return docnames


def _get_ordered_language_codes(language_sources, preferred_label_map):
    ordered_codes = []
    for code in preferred_label_map:
        normalized_code = _normalize_language_code(code)
        if normalized_code in language_sources and normalized_code not in ordered_codes:
            ordered_codes.append(normalized_code)

    for code in sorted(language_sources):
        if code not in ordered_codes:
            ordered_codes.append(code)
    return ordered_codes


def _resolve_language_labels(ordered_codes, preferred_label_map):
    language_labels = {}
    for code in ordered_codes:
        label = preferred_label_map.get(code)
        if not label:
            label = code.upper() if len(code) <= 3 else code
        language_labels[code] = label
    return language_labels


def _resolve_catalog_language_key(catalog_keys, language_code, default_language):
    keys = list(catalog_keys)
    if not keys:
        return ""

    normalized_code = _normalize_language_code(language_code)
    if normalized_code in keys:
        return normalized_code

    normalized_base = normalized_code.split("-", 1)[0] if normalized_code else ""
    if normalized_base:
        if normalized_base in keys:
            return normalized_base
        for candidate in keys:
            if candidate.split("-", 1)[0] == normalized_base:
                return candidate

    normalized_default = _normalize_language_code(default_language)
    if normalized_default in keys:
        return normalized_default

    default_base = normalized_default.split("-", 1)[0] if normalized_default else ""
    if default_base:
        if default_base in keys:
            return default_base
        for candidate in keys:
            if candidate.split("-", 1)[0] == default_base:
                return candidate

    return keys[0]


def _resolve_text_catalog(catalog, language_code, default_language):
    normalized_catalog = {}
    for raw_code, raw_texts in catalog.items():
        normalized_code = _normalize_language_code(raw_code)
        if not normalized_code or not isinstance(raw_texts, dict):
            continue
        normalized_catalog[normalized_code] = dict(raw_texts)

    if not normalized_catalog:
        return {}

    default_key = _resolve_catalog_language_key(
        normalized_catalog.keys(),
        default_language,
        default_language,
    )
    target_key = _resolve_catalog_language_key(
        normalized_catalog.keys(),
        language_code,
        default_language,
    )

    resolved = dict(normalized_catalog.get(default_key, {}))
    resolved.update(normalized_catalog.get(target_key, {}))
    return resolved


def _resolve_ui_text(language_code):
    return _resolve_text_catalog(
        cs_ui_text_catalog,
        language_code,
        cs_ui_default_language,
    )


def _resolve_root_index_text(language_code):
    return _resolve_text_catalog(
        cs_root_index_text_catalog,
        language_code,
        cs_root_index_default_language,
    )


def _build_language_context(config):
    source_root = os.path.dirname(__file__)
    language_sources = _discover_language_sources(source_root)
    ordered_codes = _get_ordered_language_codes(language_sources, cs_language_label_map)
    language_labels = _resolve_language_labels(ordered_codes, cs_language_label_map)

    docs_by_language = {}
    for code in ordered_codes:
        docs_by_language[code] = _collect_docnames(language_sources[code])

    return ordered_codes, language_labels, docs_by_language


def _apply_language_project_name(app, config):
    lang = (config.language or "").lower()
    if lang.startswith("zh"):
        config.project = "CrazyStorm 2.0 教程"
    else:
        config.project = "CrazyStorm 2.0 Tutorial"


def _prepare_language_links(app, config):
    global _cs_language_codes
    global _cs_language_labels
    global _cs_language_docs

    _cs_language_codes, _cs_language_labels, _cs_language_docs = _build_language_context(config)


def _inject_page_language_links(app, pagename, templatename, context, doctree):
    ui_text = _resolve_ui_text(app.config.language)
    context["cs_ui_text"] = ui_text
    context["cs_ui_text_json"] = json.dumps(ui_text, ensure_ascii=False)

    if not _cs_language_codes:
        context["cs_language_links"] = []
        return

    current_language = _normalize_language_code(app.config.language)
    current_docname = _normalize_docname(pagename)
    root_docname = _normalize_docname(getattr(app.config, "root_doc", "") or master_doc)

    path_depth = current_docname.count("/")
    root_prefix = "../" * (path_depth + 1)

    language_links = []
    for language_code in _cs_language_codes:
        target_docname = current_docname
        target_docs = _cs_language_docs.get(language_code, set())
        if current_docname not in target_docs:
            target_docname = root_docname

        is_current = language_code == current_language
        item = {
            "code": language_code,
            "label": _cs_language_labels.get(language_code, language_code),
            "href": "",
            "is_current": is_current,
            "is_fallback": target_docname != current_docname,
        }
        if not is_current:
            item["href"] = "{}{}/{}.html".format(root_prefix, language_code, target_docname)
        language_links.append(item)

    context["cs_language_links"] = language_links


def _build_root_index_language_links(app):
    source_root = os.path.dirname(__file__)
    language_sources = _discover_language_sources(source_root)
    ordered_codes = _get_ordered_language_codes(language_sources, cs_root_language_label_map)
    language_labels = _resolve_language_labels(ordered_codes, cs_root_language_label_map)

    docs_root = os.path.dirname(os.path.abspath(app.outdir))
    language_links = []
    for code in ordered_codes:
        output_index = os.path.join(docs_root, code, "index.html")
        if not os.path.isfile(output_index):
            continue

        language_links.append({
            "code": code,
            "label": language_labels.get(code, code),
            "href": "./{}/index.html".format(code),
        })

    return language_links


def _build_root_index_runtime_text_catalog(language_links):
    runtime_catalog = {}
    for language_link in language_links:
        language_code = _normalize_language_code(language_link.get("code"))
        if not language_code or language_code in runtime_catalog:
            continue
        runtime_catalog[language_code] = _resolve_root_index_text(language_code)

    default_code = _normalize_language_code(cs_root_index_default_language)
    if default_code and default_code not in runtime_catalog:
        runtime_catalog[default_code] = _resolve_root_index_text(default_code)

    if not runtime_catalog:
        fallback_code = default_code or "en"
        runtime_catalog[fallback_code] = _resolve_root_index_text(fallback_code)

    return runtime_catalog


def _render_root_index_html(language_links):
    default_code = _normalize_language_code(cs_root_index_default_language) or "en"
    default_text = _resolve_root_index_text(default_code)
    if not default_text:
        default_text = {
            "title": "CrazyStorm 2.0 Tutorial Entry",
            "heading": "CrazyStorm 2.0 Tutorial",
            "tip": "This is the local offline tutorial entry page.",
            "choose_language": "Choose Language",
            "empty": "No language docs are currently built. Please run Docs/build.ps1 first.",
            "html_lang": "en",
        }

    runtime_catalog = _build_root_index_runtime_text_catalog(language_links)
    runtime_catalog_json = json.dumps(runtime_catalog, ensure_ascii=False).replace("</", "<\\/")
    default_code_json = json.dumps(default_code, ensure_ascii=False)

    html_lines = [
        "<!DOCTYPE html>",
        '<html lang="{}">'.format(escape(default_text.get("html_lang", "en"), quote=True)),
        "<head>",
        '    <meta charset="utf-8" />',
        '    <meta name="viewport" content="width=device-width, initial-scale=1" />',
        "    <title>{}</title>".format(escape(default_text.get("title", ""))),
        "    <style>",
        '        body { font-family: "Segoe UI", "Microsoft YaHei", sans-serif; margin: 40px; color: #1f2937; }',
        "        .card { max-width: 760px; border: 1px solid #d1d5db; border-radius: 8px; padding: 24px; background: #ffffff; }",
        "        h1 { margin-top: 0; }",
        "        ul { line-height: 1.9; }",
        "        .tip { color: #4b5563; }",
        "        .empty { color: #6b7280; }",
        "        a { color: #0f4c81; text-decoration: none; }",
        "        a:hover { text-decoration: underline; }",
        "    </style>",
        "</head>",
        "<body>",
        '    <div class="card">',
        '        <h1 id="cs-root-heading">{}</h1>'.format(escape(default_text.get("heading", ""))),
        '        <p id="cs-root-tip" class="tip">{}</p>'.format(escape(default_text.get("tip", ""))),
        '        <h2 id="cs-root-choose-language">{}</h2>'.format(escape(default_text.get("choose_language", ""))),
    ]

    if language_links:
        html_lines.append("        <ul>")
        for language_link in language_links:
            href = escape(language_link["href"], quote=True)
            label = escape(language_link["label"])
            html_lines.append('            <li><a href="{}">{}</a></li>'.format(href, label))
        html_lines.append("        </ul>")
    else:
        html_lines.append(
            '        <p id="cs-root-empty" class="empty">{}</p>'.format(escape(default_text.get("empty", "")))
        )

    html_lines.extend([
        "    </div>",
        "    <script>",
        "    (function () {",
        '        "use strict";',
        "        var textCatalog = " + runtime_catalog_json + ";",
        "        var defaultLanguage = " + default_code_json + ";",
        "",
        "        function normalizeLanguageCode(value) {",
        "            if (!value) return \"\";",
        "            return String(value).trim().toLowerCase().replace(/_/g, \"-\");",
        "        }",
        "",
        "        function resolveCatalogLanguage(languageCode) {",
        "            var normalized = normalizeLanguageCode(languageCode);",
        "            if (normalized && Object.prototype.hasOwnProperty.call(textCatalog, normalized)) return normalized;",
        "",
        "            var base = normalized ? normalized.split(\"-\", 1)[0] : \"\";",
        "            if (base && Object.prototype.hasOwnProperty.call(textCatalog, base)) return base;",
        "",
        "            if (base) {",
        "                for (var key in textCatalog) {",
        "                    if (!Object.prototype.hasOwnProperty.call(textCatalog, key)) continue;",
        "                    if (key.split(\"-\", 1)[0] === base) return key;",
        "                }",
        "            }",
        "",
        "            var normalizedDefault = normalizeLanguageCode(defaultLanguage);",
        "            if (normalizedDefault && Object.prototype.hasOwnProperty.call(textCatalog, normalizedDefault)) {",
        "                return normalizedDefault;",
        "            }",
        "",
        "            for (var fallbackKey in textCatalog) {",
        "                if (Object.prototype.hasOwnProperty.call(textCatalog, fallbackKey)) return fallbackKey;",
        "            }",
        "            return \"\";",
        "        }",
        "",
        "        function setTextById(id, value) {",
        "            if (!value) return;",
        "            var element = document.getElementById(id);",
        "            if (!element) return;",
        "            element.textContent = value;",
        "        }",
        "",
        "        var preferredLanguage = navigator.language || navigator.userLanguage || \"\";",
        "        var resolvedLanguage = resolveCatalogLanguage(preferredLanguage);",
        "        if (!resolvedLanguage) return;",
        "",
        "        var localizedText = textCatalog[resolvedLanguage] || {};",
        "        if (localizedText.title) document.title = localizedText.title;",
        "        if (localizedText.html_lang) document.documentElement.setAttribute(\"lang\", localizedText.html_lang);",
        "",
        "        setTextById(\"cs-root-heading\", localizedText.heading);",
        "        setTextById(\"cs-root-tip\", localizedText.tip);",
        "        setTextById(\"cs-root-choose-language\", localizedText.choose_language);",
        "        setTextById(\"cs-root-empty\", localizedText.empty);",
        "    })();",
        "    </script>",
        "</body>",
        "</html>",
        "",
    ])
    return "\n".join(html_lines)


def _generate_root_index(app, exception):
    if exception is not None:
        return

    docs_root = os.path.dirname(os.path.abspath(app.outdir))
    output_path = os.path.join(docs_root, "index.html")
    language_links = _build_root_index_language_links(app)
    html_text = _render_root_index_html(language_links)
    with open(output_path, "w", encoding="utf-8", newline="\n") as output_file:
        output_file.write(html_text)


def setup(app):
    app.connect("config-inited", _apply_language_project_name)
    app.connect("config-inited", _prepare_language_links)
    app.connect("html-page-context", _inject_page_language_links)
    app.connect("build-finished", _generate_root_index)
