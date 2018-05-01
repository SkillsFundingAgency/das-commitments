var sfa = sfa || {};

sfa.settings = {
  init: function() {
    document.body.className = document.body.className
      ? document.body.className + " js-enabled"
      : "js-enabled";
  }
};

sfa.tabs = {
  elems: {
    tabs: $("ul.js-tabs li a"),
    panels: $(".js-tab-pane")
  },

  init: function() {
    if (this.elems.tabs) {
      this.setUpEvents(this.elems.tabs);
      this.hidePanels(this.elems.panels);
    }

    this.elems.tabs.eq(0).click();
  },

  hidePanels: function(panels) {
    panels.hide();
  },

  showPanel: function(panel) {
    panel.show();
  },

  setUpEvents: function(tabs) {
    var that = this;

    tabs.on("click touchstart", function(e) {
      tabs.closest("li").removeClass("current");
      $(this)
        .closest("li")
        .addClass("current");

      var target = $(this).attr("href");

      that.hidePanels(that.elems.panels);
      that.showPanel($(target));

      e.preventDefault();
    });
  }
};

sfa.focusSwitch = {
  init: function() {
    var fields = $(".focus-switch");

    fields.on("keyup", function() {
      var that = $(this),
        length = that.val().length,
        maxlength = that.attr("maxlength"),
        nextid = that.data("next-id");

      if (length == maxlength) {
        $("#" + nextid).focus();
      }
    });
  }
};

sfa.settings.init();
sfa.tabs.init();

if ($(".focus-switch").length > 0) {
  sfa.focusSwitch.init();
}

(function() {
  "use strict";

  // header navigation toggle
  if (document.querySelectorAll && document.addEventListener) {
    var els = document.querySelectorAll(".js-header-toggle"),
      i,
      _i;
    for (i = 0, _i = els.length; i < _i; i++) {
      els[i].addEventListener("click", function(e) {
        e.preventDefault();
        var target = document.getElementById(
            this.getAttribute("href").substr(1)
          ),
          targetClass = target.getAttribute("class") || "",
          sourceClass = this.getAttribute("class") || "";

        if (targetClass.indexOf("js-visible") !== -1) {
          target.setAttribute(
            "class",
            targetClass.replace(/(^|\s)js-visible(\s|$)/, "")
          );
        } else {
          target.setAttribute("class", targetClass + " js-visible");
        }
        if (sourceClass.indexOf("js-hidden") !== -1) {
          this.setAttribute(
            "class",
            sourceClass.replace(/(^|\s)js-hidden(\s|$)/, "")
          );
        } else {
          this.setAttribute("class", sourceClass + " js-hidden");
        }
      });
    }
  }
}.call(this));
