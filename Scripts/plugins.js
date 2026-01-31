/* jQuery Plugins */
(function ($) {
    /* Copyright (c) 2010 Brandon Aaron (http://brandonaaron.net)
     * Licensed under the MIT License (LICENSE.txt).
     *
     * Thanks to: http://adomas.org/javascript-mouse-wheel/ for some pointers.
     * Thanks to: Mathias Bank(http://www.mathias-bank.de) for a scope bug fix.
     * Thanks to: Seamus Leahy for adding deltaX and deltaY
     *
     * Version: 3.0.4
     *
     * Requires: 1.2.2+
     */
    var types = ["DOMMouseScroll", "mousewheel"]; $.event.special.mousewheel = { setup: function () { if (this.addEventListener) { for (var a = types.length; a;) { this.addEventListener(types[--a], handler, false) } } else { this.onmousewheel = handler } }, teardown: function () { if (this.removeEventListener) { for (var a = types.length; a;) { this.removeEventListener(types[--a], handler, false) } } else { this.onmousewheel = null } } }; $.fn.extend({ mousewheel: function (a) { return a ? this.bind("mousewheel", a) : this.trigger("mousewheel") }, unmousewheel: function (a) { return this.unbind("mousewheel", a) } }); function handler(f) { var d = f || window.event, c = [].slice.call(arguments, 1), g = 0, e = true, b = 0, a = 0; f = $.event.fix(d); f.type = "mousewheel"; if (f.wheelDelta) { g = f.wheelDelta / 120 } if (f.detail) { g = -f.detail / 3 } a = g; if (d.axis !== undefined && d.axis === d.HORIZONTAL_AXIS) { a = 0; b = -1 * g } if (d.wheelDeltaY !== undefined) { a = d.wheelDeltaY / 120 } if (d.wheelDeltaX !== undefined) { b = -1 * d.wheelDeltaX / 120 } c.unshift(f, g, b, a); return $.event.handle.apply(this, c) };
    /*
    * hoverIntent is similar to jQuery's built-in "hover" function except that
    * instead of firing the onMouseOver event immediately, hoverIntent checks
    * to see if the user's mouse has slowed down (beneath the sensitivity
    * threshold) before firing the onMouseOver event.
    *
    * hoverIntent r6 // 2011.02.26 // jQuery 1.5.1+
    * <http://cherne.net/brian/resources/jquery.hoverIntent.html>
    *
    * hoverIntent is currently available for use in all personal or commercial
    * projects under both MIT and GPL licenses. This means that you can choose
    * the license that best suits your project, and use it accordingly.
    */
    $.fn.hoverIntent = function (j, i) { var k = { sensitivity: 7, interval: 100, timeout: 0 }; k = $.extend(k, i ? { over: j, out: i } : j); var m, l, e, c; var d = function (f) { m = f.pageX; l = f.pageY }; var b = function (g, f) { f.hoverIntent_t = clearTimeout(f.hoverIntent_t); if ((Math.abs(e - m) + Math.abs(c - l)) < k.sensitivity) { $(f).unbind("mousemove", d); f.hoverIntent_s = 1; return k.over.apply(f, [g]) } else { e = m; c = l; f.hoverIntent_t = setTimeout(function () { b(g, f) }, k.interval) } }; var h = function (g, f) { f.hoverIntent_t = clearTimeout(f.hoverIntent_t); f.hoverIntent_s = 0; return k.out.apply(f, [g]) }; var a = function (n) { var g = jQuery.extend({}, n); var f = this; if (f.hoverIntent_t) { f.hoverIntent_t = clearTimeout(f.hoverIntent_t) } if (n.type == "mouseenter") { e = g.pageX; c = g.pageY; $(f).bind("mousemove", d); if (f.hoverIntent_s != 1) { f.hoverIntent_t = setTimeout(function () { b(g, f) }, k.interval) } } else { $(f).unbind("mousemove", d); if (f.hoverIntent_s == 1) { f.hoverIntent_t = setTimeout(function () { h(g, f) }, k.timeout) } } }; return this.bind("mouseenter", a).bind("mouseleave", a) };
    /*
    * jQuery plugin from http://remysharp.com/tag/marquee that makes the marquee tag work correctly in modern browsers.
    */
    $.fn.marquee = function (a) { var d = [], c = this.length; function b(l, j, k) { var i = k.behavior, g = k.width, f = k.dir; var h = 0; if (i == "alternate") { h = l == 1 ? j[k.widthAxis] - (g * 2) : g } else { if (i == "slide") { if (l == -1) { h = f == -1 ? j[k.widthAxis] : g } else { h = f == -1 ? j[k.widthAxis] - (g * 2) : 0 } } else { h = l == -1 ? j[k.widthAxis] : 0 } } return h } function e() { var g = d.length, h = null, l = null, k = {}, j = [], f = false; while (g--) { h = d[g]; l = $(h); k = l.data("marqueeState"); if (l.data("paused") !== true) { h[k.axis] += (k.scrollamount * k.dir); f = k.dir == -1 ? h[k.axis] <= b(k.dir * -1, h, k) : h[k.axis] >= b(k.dir * -1, h, k); if ((k.behavior == "scroll" && k.last == h[k.axis]) || (k.behavior == "alternate" && f && k.last != -1) || (k.behavior == "slide" && f && k.last != -1)) { if (k.behavior == "alternate") { k.dir *= -1 } k.last = -1; l.trigger("stop"); k.loops--; if (k.loops === 0) { if (k.behavior != "slide") { h[k.axis] = b(k.dir, h, k) } else { h[k.axis] = b(k.dir * -1, h, k) } l.trigger("end") } else { j.push(h); l.trigger("start"); h[k.axis] = b(k.dir, h, k) } } else { j.push(h) } k.last = h[k.axis]; l.data("marqueeState", k) } else { j.push(h) } } d = j; if (d.length) { setTimeout(e, 25) } } this.each(function (h) { var m = $(this), f = m.attr("width") || m.width(), n = m.attr("height") || m.height(), wwu = (typeof (f) == 'string' ? f : f + 'px'), hwu = (typeof (n) == 'string' ? n : n + 'px'), o = m.after("<div " + (a ? 'class="' + a + '" ' : "") + 'style="display: inline-block; width: ' + wwu + "; height: " + hwu + '; overflow: hidden;"><div style="float: left; white-space: nowrap;">' + m.html() + "</div></div>").next(), l = o.get(0), j = 0, k = (m.attr("direction") || "left").toLowerCase(), g = { dir: /down|right/.test(k) ? -1 : 1, axis: /left|right/.test(k) ? "scrollLeft" : "scrollTop", widthAxis: /left|right/.test(k) ? "scrollWidth" : "scrollHeight", last: -1, loops: m.attr("loop") || -1, scrollamount: m.attr("scrollamount") || this.scrollAmount || 2, behavior: (m.attr("behavior") || "scroll").toLowerCase(), width: /left|right/.test(k) ? f : n }; if (m.attr("loop") == -1 && g.behavior == "slide") { g.loops = 1 } m.remove(); if (/left|right/.test(k)) { o.find("> div").css("padding", "0 " + wwu) } else { o.find("> div").css("padding", hwu + " 0") } o.bind("stop", function () { o.data("paused", true) }).bind("pause", function () { o.data("paused", true) }).bind("start", function () { o.data("paused", false) }).bind("unpause", function () { o.data("paused", false) }).data("marqueeState", g); d.push(l); l[g.axis] = b(g.dir, l, g); o.trigger("start"); if (h + 1 == c) { e() } }); return $(d) };
    /*
    * jQuery placeholder for browser that dont natively support it http://mths.be/placeholder v2.0.7 by @mathias
    */
})(jQuery);