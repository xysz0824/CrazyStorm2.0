(function () {
    "use strict";

    var sidebarWrapper = null;
    var sidebarHost = null;
    var mobileNavToggle = null;
    var mobileNavBackdrop = null;
    var mobileNavMediaQuery = window.matchMedia
        ? window.matchMedia("(orientation: portrait), (max-width: 980px)")
        : null;
    var i18n = null;
    var isInitialized = false;
    var standaloneSearchStarted = false;
    var nativeSearchGuardInstalled = false;

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initializePageEnhancements);
    } else {
        initializePageEnhancements();
    }

    function initializePageEnhancements() {
        if (isInitialized) return;
        isInitialized = true;

        sidebarWrapper = document.querySelector("div.sphinxsidebarwrapper");
        sidebarHost = document.querySelector("div.sphinxsidebar");
        mobileNavToggle = document.querySelector(".cs-mobile-nav-toggle");
        mobileNavBackdrop = document.querySelector(".cs-mobile-nav-backdrop");
        i18n = resolveI18n();

        normalizeSearchSubmitLabels();
        setupSearchRecovery();

        if (!sidebarWrapper) return;

        var version = "v2.0";
        injectVersion(version);
        setupSidebarTopBandHeight();
        setupCollapsibleNav();
        setupMobileNavDrawer();
    }

    function injectVersion(text) {
        var logo = sidebarWrapper.querySelector("h1.logo");
        if (!logo) return;
        if (sidebarWrapper.querySelector(".cs-doc-version")) return;

        var versionNode = document.createElement("div");
        versionNode.className = "cs-doc-version";
        versionNode.textContent = text;
        logo.insertAdjacentElement("afterend", versionNode);
    }

    function setupCollapsibleNav() {
        var level1Items = sidebarWrapper.querySelectorAll("li.toctree-l1");
        for (var i = 0; i < level1Items.length; i++) {
            try {
                initCollapsibleItem(level1Items[i]);
            } catch (error) {
                if (window.console && typeof window.console.warn === "function") {
                    window.console.warn("cs-nav init failed:", error);
                }
            }
        }
    }

    function setupMobileNavDrawer() {
        if (!sidebarHost || !mobileNavToggle || !mobileNavBackdrop) return;

        var sidebarId = String(sidebarHost.id || "").trim();
        if (!sidebarId) {
            sidebarId = "cs-mobile-sidebar";
            sidebarHost.id = sidebarId;
        }
        mobileNavToggle.setAttribute("aria-controls", sidebarId);
        updateMobileNavToggleLabel(false);
        setMobileNavOpen(false);

        mobileNavToggle.addEventListener("click", function () {
            setMobileNavOpen(!isMobileNavOpen());
        });

        mobileNavBackdrop.addEventListener("click", function () {
            setMobileNavOpen(false);
        });

        sidebarHost.addEventListener("click", function (event) {
            if (!isMobileMode()) return;
            if (!event || !event.target || typeof event.target.closest !== "function") return;
            if (!event.target.closest("a[href]")) return;

            setMobileNavOpen(false);
        });

        document.addEventListener("keydown", function (event) {
            if (!event) return;
            if (event.key !== "Escape" && event.key !== "Esc") return;

            setMobileNavOpen(false);
        });

        if (mobileNavMediaQuery) {
            addMediaQueryChangeListener(mobileNavMediaQuery, syncMobileNavMode);
        } else {
            window.addEventListener("resize", syncMobileNavMode);
            window.addEventListener("orientationchange", syncMobileNavMode);
        }
        syncMobileNavMode();
    }

    function addMediaQueryChangeListener(mediaQueryList, listener) {
        if (!mediaQueryList || !listener) return;

        if (typeof mediaQueryList.addEventListener === "function") {
            mediaQueryList.addEventListener("change", listener);
            return;
        }
        if (typeof mediaQueryList.addListener === "function") {
            mediaQueryList.addListener(listener);
        }
    }

    function syncMobileNavMode() {
        if (!isMobileMode()) {
            setMobileNavOpen(false);
            return;
        }
        setMobileNavOpen(false);
    }

    function isMobileMode() {
        if (mobileNavMediaQuery) {
            return !!mobileNavMediaQuery.matches;
        }

        var width = getViewportWidth();
        var height = getViewportHeight();
        if (width <= 0 || height <= 0) return false;

        return width <= 980 || height > width;
    }

    function getViewportWidth() {
        if (typeof window.innerWidth === "number" && window.innerWidth > 0) {
            return window.innerWidth;
        }

        var root = document.documentElement;
        return root && root.clientWidth ? root.clientWidth : 0;
    }

    function getViewportHeight() {
        if (typeof window.innerHeight === "number" && window.innerHeight > 0) {
            return window.innerHeight;
        }

        var root = document.documentElement;
        return root && root.clientHeight ? root.clientHeight : 0;
    }

    function isMobileNavOpen() {
        return document.documentElement.classList.contains("cs-mobile-nav-open");
    }

    function setMobileNavOpen(isOpen) {
        if (!mobileNavToggle) return;

        var openState = !!isOpen && isMobileMode();
        document.documentElement.classList.toggle("cs-mobile-nav-open", openState);
        mobileNavToggle.setAttribute("aria-expanded", openState ? "true" : "false");
        updateMobileNavToggleLabel(openState);
    }

    function updateMobileNavToggleLabel(isOpen) {
        if (!mobileNavToggle) return;

        var label = isOpen ? i18n.mobile_nav_close : i18n.mobile_nav_open;
        if (!label) return;

        mobileNavToggle.setAttribute("aria-label", label);
        mobileNavToggle.setAttribute("title", label);
    }

    function initCollapsibleItem(item) {
        if (!item) return;
        if (findDirectChildByClass(item, "cs-nav-toggle")) return;

        var children = findDirectChildByTag(item, "ul");
        if (!children) return;

        var button = document.createElement("button");
        button.type = "button";
        button.className = "cs-nav-toggle";
        if (i18n.nav_toggle_aria_label) {
            button.setAttribute("aria-label", i18n.nav_toggle_aria_label);
        }
        button.textContent = "▾";
        item.appendChild(button);

        var expanded = item.classList.contains("current");
        if (!expanded) {
            item.classList.add("cs-collapsed");
        }
        button.setAttribute("aria-expanded", expanded ? "true" : "false");

        button.addEventListener("click", function (event) {
            var trigger = event.currentTarget;
            if (!trigger) return;

            var currentItem = trigger.parentElement;
            if (!currentItem) return;

            currentItem.classList.toggle("cs-collapsed");
            var isExpanded = !currentItem.classList.contains("cs-collapsed");
            trigger.setAttribute("aria-expanded", isExpanded ? "true" : "false");
        });
    }

    function findDirectChildByTag(parent, tagName) {
        if (!parent || !tagName) return null;

        var expected = String(tagName).toLowerCase();
        var children = parent.children;
        for (var i = 0; i < children.length; i++) {
            var child = children[i];
            if (child.tagName && child.tagName.toLowerCase() === expected) {
                return child;
            }
        }
        return null;
    }

    function findDirectChildByClass(parent, className) {
        if (!parent || !className) return null;

        var children = parent.children;
        for (var i = 0; i < children.length; i++) {
            var child = children[i];
            if (child.classList && child.classList.contains(className)) {
                return child;
            }
        }
        return null;
    }

    function resolveI18n() {
        var configured = window.CS_DOC_I18N;
        if (!configured || typeof configured !== "object") {
            configured = {};
        }

        return {
            search_submit: normalizeOptionalString(configured.search_submit),
            search_submit_rewrite_from: normalizeTokenList(configured.search_submit_rewrite_from),
            search_results_title: normalizeOptionalString(configured.search_results_title),
            search_no_match: normalizeOptionalString(configured.search_no_match),
            search_finished: normalizeOptionalString(configured.search_finished),
            search_tokens_preparing: normalizeTokenList(configured.search_tokens_preparing),
            search_tokens_searching: normalizeTokenList(configured.search_tokens_searching),
            search_tokens_completed: normalizeTokenList(configured.search_tokens_completed),
            nav_toggle_aria_label: normalizeOptionalString(configured.nav_toggle_aria_label),
            mobile_nav_open: normalizeOptionalString(configured.mobile_nav_open),
            mobile_nav_close: normalizeOptionalString(configured.mobile_nav_close),
            mobile_nav_label: normalizeOptionalString(configured.mobile_nav_label),
            mobile_nav_menu: normalizeOptionalString(configured.mobile_nav_menu)
        };
    }

    function normalizeOptionalString(value) {
        if (value === null || value === undefined) return "";
        return String(value);
    }

    function normalizeTokenList(value) {
        var result = [];
        if (!value || typeof value === "string" || typeof value.length !== "number") {
            return result;
        }

        for (var i = 0; i < value.length; i++) {
            var token = normalizeText(value[i]);
            if (!token || containsExactTerm(token, result)) continue;
            result.push(token);
        }
        return result;
    }

    function formatTemplate(template, values) {
        var text = String(template || "");
        return text.replace(/\{(\w+)\}/g, function (_, key) {
            if (!values || !Object.prototype.hasOwnProperty.call(values, key)) {
                return "";
            }
            return String(values[key]);
        });
    }

    function normalizeSearchSubmitLabels() {
        var buttons = document.querySelectorAll("input[type='submit']");
        if (!buttons || buttons.length === 0) return;

        var preferredLabel = String(i18n.search_submit || "");
        if (!preferredLabel) return;

        var normalizedPreferredLabel = normalizeText(preferredLabel);
        var rewriteFrom = i18n.search_submit_rewrite_from;
        for (var i = 0; i < buttons.length; i++) {
            var button = buttons[i];
            if (!button) continue;

            var form = button.form;
            if (!isSearchForm(form)) continue;

            var current = normalizeText(button.value);
            if (!current || current === normalizedPreferredLabel || containsExactTerm(current, rewriteFrom)) {
                button.value = preferredLabel;
            }
        }
    }

    function isSearchForm(form) {
        if (!form) return false;
        if (form.classList && form.classList.contains("search")) return true;

        var action = String(form.getAttribute("action") || "").toLowerCase();
        return action === "search.html" || action.indexOf("search.html") >= 0;
    }

    function setupSearchRecovery() {
        if (!isSearchPage()) return;

        if (document.readyState === "loading") {
            document.addEventListener("DOMContentLoaded", scheduleSearchRecovery);
        } else {
            scheduleSearchRecovery();
        }
    }

    function scheduleSearchRecovery() {
        if (!getQueryText()) return;

        // Start fallback immediately and retry briefly for late script init.
        tryStandaloneSearchRecovery();
        window.setTimeout(tryStandaloneSearchRecovery, 120);
        window.setTimeout(tryStandaloneSearchRecovery, 320);
    }

    function tryStandaloneSearchRecovery() {
        if (standaloneSearchStarted) return;

        var queryText = getQueryText();
        if (!queryText) return;
        if (isSearchCompleted()) return;

        standaloneSearchStarted = true;
        runStandaloneSearch(queryText);
    }

    function runStandaloneSearch(queryText) {
        installNativeSearchGuard();
        captureSearchIndex(function (indexData) {
            if (isSearchCompleted()) return;
            renderStandaloneSearchResults(queryText, indexData);
        });
    }

    function captureSearchIndex(callback) {
        var nativeSearch = getNativeSearchApi();
        if (nativeSearch && nativeSearch._index) {
            callback(nativeSearch._index);
            return;
        }

        var capturedIndex = null;
        var restore = null;

        if (nativeSearch && typeof nativeSearch.setIndex === "function") {
            var originalSetIndex = nativeSearch.setIndex;
            nativeSearch.setIndex = function (index) {
                capturedIndex = index;
                return originalSetIndex.call(nativeSearch, index);
            };
            restore = function () {
                nativeSearch.setIndex = originalSetIndex;
            };
        } else {
            var previousSearch = window.Search;
            window.Search = {
                setIndex: function (index) {
                    capturedIndex = index;
                }
            };
            restore = function () {
                window.Search = previousSearch;
            };
        }

        var script = document.createElement("script");
        script.src = resolveSearchIndexUrl();
        script.async = true;
        script.onload = function () {
            if (restore) restore();
            callback(capturedIndex);
        };
        script.onerror = function () {
            if (restore) restore();
            callback(capturedIndex);
        };
        document.body.appendChild(script);
    }

    function resolveSearchIndexUrl() {
        var root = document.documentElement.getAttribute("data-content_root") || "./";
        return root + "searchindex.js";
    }

    function renderStandaloneSearchResults(queryText, indexData) {
        if (isSearchCompleted()) return;

        var output = document.getElementById("search-results");
        if (!output) return;

        var nativeSearch = getNativeSearchApi();
        if (nativeSearch && typeof nativeSearch.stopPulse === "function") {
            nativeSearch.stopPulse();
        }

        var progress = document.getElementById("search-progress");
        if (progress) progress.innerText = "";

        while (output.firstChild) {
            output.removeChild(output.firstChild);
        }

        var titleNode = document.createElement("h2");
        titleNode.textContent = i18n.search_results_title;
        output.appendChild(titleNode);

        var summaryNode = document.createElement("p");
        summaryNode.className = "search-summary";
        output.appendChild(summaryNode);

        var listNode = document.createElement("ul");
        listNode.className = "search";
        listNode.setAttribute("role", "list");
        output.appendChild(listNode);

        var results = performStandaloneQuery(indexData, queryText);
        if (!results || results.length === 0) {
            summaryNode.textContent = i18n.search_no_match;
            return;
        }

        summaryNode.textContent = formatTemplate(i18n.search_finished, {
            count: results.length
        });

        for (var i = 0; i < results.length; i++) {
            var li = document.createElement("li");
            var link = document.createElement("a");
            link.href = results[i].href;
            link.textContent = results[i].title;
            li.appendChild(link);
            listNode.appendChild(li);
        }

        // Some engines execute native search callbacks later; keep status clean.
        clearSearchLoadingArtifacts();
        window.setTimeout(clearSearchLoadingArtifacts, 50);
        window.setTimeout(clearSearchLoadingArtifacts, 180);
        window.setTimeout(clearSearchLoadingArtifacts, 420);
    }

    function performStandaloneQuery(indexData, queryText) {
        if (!indexData) return [];

        var queryTerms = buildQueryTerms(queryText);
        if (queryTerms.length === 0) return [];

        var scores = {};
        addTitleHits(indexData, queryTerms, scores);
        addAllTitleHits(indexData, queryTerms, scores);
        addTermMapHits(indexData.titleterms, queryTerms, scores, 6);
        addTermMapHits(indexData.terms, queryTerms, scores, 3);

        var docnames = indexData.docnames || [];
        var titles = indexData.titles || [];
        var results = [];

        for (var rawId in scores) {
            if (!Object.prototype.hasOwnProperty.call(scores, rawId)) continue;

            var id = Number(rawId);
            if (!isFinite(id)) continue;

            var docName = docnames[id];
            if (!docName) continue;

            var title = titles[id] || docName;
            results.push({
                id: id,
                score: scores[rawId],
                title: title,
                href: docName + ".html"
            });
        }

        results.sort(function (left, right) {
            if (right.score !== left.score) return right.score - left.score;

            var leftTitle = String(left.title || "").toLowerCase();
            var rightTitle = String(right.title || "").toLowerCase();
            if (leftTitle < rightTitle) return -1;
            if (leftTitle > rightTitle) return 1;
            return 0;
        });

        var limit = 80;
        if (results.length > limit) {
            return results.slice(0, limit);
        }
        return results;
    }

    function buildQueryTerms(queryText) {
        var normalized = normalizeText(queryText);
        if (!normalized) return [];

        var terms = [];
        var seen = {};

        addUniqueTerm(terms, seen, normalized);

        var parts = normalized.split(/\s+/);
        for (var i = 0; i < parts.length; i++) {
            addUniqueTerm(terms, seen, parts[i]);
        }
        return terms;
    }

    function addUniqueTerm(terms, seen, term) {
        var normalized = normalizeText(term);
        if (!normalized) return;
        if (Object.prototype.hasOwnProperty.call(seen, normalized)) return;

        seen[normalized] = true;
        terms.push(normalized);
    }

    function normalizeText(value) {
        if (value === null || value === undefined) return "";
        return String(value).toLowerCase().replace(/^\s+|\s+$/g, "");
    }

    function addTitleHits(indexData, queryTerms, scores) {
        var titles = indexData.titles || [];
        for (var i = 0; i < titles.length; i++) {
            var title = normalizeText(titles[i]);
            if (!title) continue;

            if (containsAnyTerm(title, queryTerms)) {
                addScore(scores, i, 20);
            }
        }
    }

    function addAllTitleHits(indexData, queryTerms, scores) {
        var allTitles = indexData.alltitles;
        if (!allTitles) return;

        for (var key in allTitles) {
            if (!Object.prototype.hasOwnProperty.call(allTitles, key)) continue;
            if (!containsAnyTerm(normalizeText(key), queryTerms)) continue;

            var records = allTitles[key];
            if (!records || typeof records.length !== "number") continue;

            for (var i = 0; i < records.length; i++) {
                var record = records[i];
                if (!record || typeof record.length !== "number" || record.length === 0) continue;
                addScore(scores, record[0], 10);
            }
        }
    }

    function addTermMapHits(termMap, queryTerms, scores, baseScore) {
        if (!termMap) return;

        for (var key in termMap) {
            if (!Object.prototype.hasOwnProperty.call(termMap, key)) continue;
            if (!containsAnyTerm(normalizeText(key), queryTerms)) continue;
            addRecordHits(termMap[key], scores, baseScore);
        }
    }

    function addRecordHits(record, scores, baseScore) {
        if (record === null || record === undefined) return;

        if (typeof record === "number") {
            addScore(scores, record, baseScore);
            return;
        }

        if (typeof record.length !== "number") return;
        for (var i = 0; i < record.length; i++) {
            addScore(scores, record[i], baseScore);
        }
    }

    function addScore(scores, docId, delta) {
        var id = Number(docId);
        if (!isFinite(id)) return;

        var key = String(id);
        if (!Object.prototype.hasOwnProperty.call(scores, key)) {
            scores[key] = 0;
        }
        scores[key] += delta;
    }

    function containsAnyTerm(target, queryTerms) {
        if (!target) return false;

        for (var i = 0; i < queryTerms.length; i++) {
            if (target.indexOf(queryTerms[i]) >= 0) {
                return true;
            }
        }
        return false;
    }

    function containsExactTerm(target, queryTerms) {
        if (!target) return false;

        for (var i = 0; i < queryTerms.length; i++) {
            if (target === queryTerms[i]) {
                return true;
            }
        }
        return false;
    }

    function getQueryText() {
        var queryString = window.location.search || "";
        if (!queryString) return "";

        var match = queryString.match(/[?&]q=([^&]*)/i);
        if (!match || match.length < 2) return "";

        try {
            return decodeURIComponent(match[1].replace(/\+/g, " "));
        } catch (error) {
            return "";
        }
    }

    function isSearchStillRunning() {
        var nativeSearch = getNativeSearchApi();
        if (nativeSearch && typeof nativeSearch._pulse_status === "number" && nativeSearch._pulse_status >= 0) {
            return true;
        }

        var title = document.querySelector("#search-results h2");
        if (!title || !title.textContent) return false;

        var text = normalizeText(title.textContent);
        return containsAnyTerm(text, i18n.search_tokens_searching);
    }

    function isSearchCompleted() {
        if (isSearchStillRunning()) return false;

        var results = document.querySelectorAll("#search-results ul.search li");
        if (results && results.length > 0) return true;

        var title = document.querySelector("#search-results h2");
        if (!title || !title.textContent) return false;

        var text = normalizeText(title.textContent);
        return containsAnyTerm(text, i18n.search_tokens_completed);
    }

    function getNativeSearchApi() {
        try {
            return Search;
        } catch (error) {
            return null;
        }
    }

    function installNativeSearchGuard() {
        if (nativeSearchGuardInstalled) return;

        var attempts = 0;
        var maxAttempts = 20;

        var tryInstall = function () {
            attempts++;
            var nativeSearch = getNativeSearchApi();
            if (nativeSearch && typeof nativeSearch.performSearch === "function") {
                if (!nativeSearch._csOriginalPerformSearch) {
                    nativeSearch._csOriginalPerformSearch = nativeSearch.performSearch;
                    nativeSearch.performSearch = function (query) {
                        if (standaloneSearchStarted) {
                            clearSearchLoadingArtifacts();
                            return;
                        }
                        return nativeSearch._csOriginalPerformSearch.call(nativeSearch, query);
                    };
                }

                nativeSearchGuardInstalled = true;
                return;
            }

            if (attempts < maxAttempts) {
                window.setTimeout(tryInstall, 50);
            }
        };

        tryInstall();
    }

    function setupSidebarTopBandHeight() {
        if (document.readyState === "loading") {
            document.addEventListener("DOMContentLoaded", applySidebarTopBandHeight);
        } else {
            applySidebarTopBandHeight();
        }

        window.addEventListener("load", applySidebarTopBandHeight);
        window.addEventListener("resize", applySidebarTopBandHeight);
    }

    function applySidebarTopBandHeight() {
        var wrapper = document.querySelector("div.sphinxsidebarwrapper");
        if (!wrapper) return;

        var searchBox = wrapper.querySelector("#searchbox");
        if (!searchBox || searchBox.offsetHeight <= 0) return;

        var styles = window.getComputedStyle(searchBox);
        var marginBottom = parseFloat(styles.marginBottom || "0");
        if (!isFinite(marginBottom)) marginBottom = 0;

        var height = searchBox.offsetTop + searchBox.offsetHeight + marginBottom + 1;
        if (!isFinite(height) || height <= 0) return;

        wrapper.style.setProperty("--bg-side-top-height", String(height) + "px");
    }

    function clearSearchLoadingArtifacts() {
        var progress = document.getElementById("search-progress");
        if (progress) progress.innerText = "";

        var output = document.getElementById("search-results");
        if (!output) return;

        var nodes = output.querySelectorAll("h2, p.search-summary");
        for (var i = 0; i < nodes.length; i++) {
            var node = nodes[i];
            var text = normalizeText(node.textContent);
            if (!text) continue;

            if (containsAnyTerm(text, i18n.search_tokens_preparing)) {
                node.textContent = "";
                continue;
            }

            if (containsExactTerm(text, i18n.search_tokens_searching)) {
                node.textContent = "";
            }
        }
    }

    function isSearchPage() {
        var path = String(window.location.pathname || "").toLowerCase();
        return path.indexOf("search.html") >= 0;
    }
})();
