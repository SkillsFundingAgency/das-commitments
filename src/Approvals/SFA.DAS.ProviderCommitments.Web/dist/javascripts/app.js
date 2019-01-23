var sfa = sfa || {};
    
sfa.homePage = {
    init: function () {
        this.startButton();
        this.toggleRadios();
    },
    startButton: function () {
        var that = this;
        $('#create_account').on('click touchstart', function (e) {
            var isYesClicked = $('#everything-yes').prop('checked'),
                errorShown = $('body').data('shownError') || false;
            if (!isYesClicked && !errorShown) {
                e.preventDefault();
                that.showError();
            }
        });
    }, 
    showError: function() {
        $('#have-not-got-everything').removeClass("js-hidden").attr("aria-hidden");
        $('body').data('shownError', true);
    },
    toggleRadios: function () {
        var radios = $('input[type=radio][name=everything-you-need]');
        radios.on('change', function () {

            radios.each(function () {
                if ($(this).prop('checked')) {
                    var target = $(this).parent().data("target");
                    $("#" + target).removeClass("js-hidden").attr("aria-hidden");
                } else {
                    var target = $(this).parent().data("target");
                    $("#" + target).addClass("js-hidden").attr("aria-hidden", "true");
                }
            });

        });
    }
}

var selectionButtons = new GOVUK.SelectionButtons("label input[type='radio'], label input[type='checkbox']");

// cohorts bingo balls - clickable block
$(".clickable").on('click touchstart', (function () {
    window.location = $(this).find("a").attr("href");
    return false;
}));

// apprentice filter page :: expand/collapse functionality
$('.container-head').on('click touchstart',(function () {
    $(this).toggleClass('showHide');
    $(this).next().toggleClass("hideOptions");

}));

//floating menu
$(window).scroll(function () {
    if ($(window).scrollTop() >= 140) {
        $('#floating-menu').addClass('fixed-header');
    }
    else {
        $('#floating-menu').removeClass('fixed-header');
    }
});


//clear search box text

var placeholderText = $('.js-enabled #search-input').data('default-value');

window.onload = function () {

    if ($('.js-enabled #search-input').val() === "") {
        $('.js-enabled #search-input').addClass('placeholder-text');
        $('.js-enabled #search-input').val(placeholderText);
    }
};

$("#search-input").on("focus click touchstart", (function () {
    $('.js-enabled #search-input').removeClass('placeholder-text');
    if ($(this).val() === placeholderText)
        $(this).val("");
}));

$("#search-input").on("blur", (function () {
    if ($(this).val() === "") {
        $('.js-enabled #search-input').addClass('placeholder-text');
        $(this).val(placeholderText);
    }
}));

