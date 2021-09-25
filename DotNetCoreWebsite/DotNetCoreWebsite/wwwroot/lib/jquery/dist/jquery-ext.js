jQuery.fn.visible = function () {
    return this.css('visibility', 'visible');
};

jQuery.fn.invisible = function () {
    return this.css('visibility', 'hidden');
};

jQuery.fn.setVisible = function (isVisible) {
    return this.css('visibility', isVisible ? 'visible' :'hidden');
};