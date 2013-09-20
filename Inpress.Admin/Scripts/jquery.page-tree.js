$.fn.buildTree = function (pageSettings) {


    /* GLOBAL VARIABLES 
    ***************************************************************************************************************/

    var treeRoot = this;

    /* DEFAULTS
    ***************************************************************************************************************/

    var pageDefaults = {
        updateElement: '.validateTips',
        ajaxMethod: '/Pages/GetPageTree',
        expanded: false,
        expandFirstChild: true,
        selectable: true,
        selectChildren: true,
        multiSelect: true,
        selectedPage: null,
        useUi: true
    };

    var pageSettings = $.extend(pageDefaults, pageSettings);


    /* CALLS
    ***************************************************************************************************************/

    buildTree();


    /* EVENTS
    ***************************************************************************************************************/

    $('li > div.item', treeRoot).live('mouseenter', function () {
        $(this).addClass('hover');
    }).live('mouseleave', function () {
        $(this).removeClass('hover');
    });

    if (pageSettings.selectable) {
        $('li > div.item > span.title', treeRoot).live('click', function () {
            if (pageSettings.selectChildren) {
                toggleChildren($(this).parent('div'));
            } else {
                toggleActive($(this).parent('div'));
            }
        });

        $('li > span.tick', treeRoot).live('click', function () {
            if (pageSettings.selectChildren) {
                toggleChildren($(this).parent('li').find('div.item:first'));
            } else {
                toggleActive($(this).parent('li').find('div.item:first'));
            }
        });
    } else {
        // do nothing for now
    }

    $('li > div.item > span.arr', treeRoot).live('click', function () {
        var arr = $(this);
        var ul = $(this).parent('div').parent().find('ul').first();

        if (!arr.hasClass('expanded')) {
            arr.addClass('expanded');
            ul.show();
        } else {
            arr.removeClass('expanded');
            ul.hide();
        }
    });


    /* FUNCTIONS
    ***************************************************************************************************************/

    function toggleChildren(item) {
        if (item.hasClass('active')) {
            // Remove the active state
            item.removeClass('child');
            item.parent('li').find('.item').removeClass('active');
            item.parent('li').find('ul .item').removeClass('child');
            item.parent('li').find('.tick').css('background-position', '0px 0px');
        } else {
            // Mark child active
            item.parent('li').find('.item').addClass('active');
            item.parent('li').find('ul .item').addClass('child');
            item.parent('li').find('.tick').css('background-position', '0px -28px');

            if (!item.parent('li').find('span', '.item').first().hasClass('indent1')) {
                item.addClass('child');
            }
        }
    };

    function toggleActive(item) {
        if (item.hasClass('active')) {
            item.removeClass('active');
            item.parent('li').find('.tick:first').css('background-position', '0px 0px');
        } else {
            if (!pageSettings.multiSelect) {
                $('.active').removeClass('active');
                $('.tick').css('background-position', '0px 0px')
            }

            item.addClass('active');
            item.parent('li').find('.tick:first').css('background-position', '0px -28px');
        }
    };

    function expandTree() {
        if (pageSettings.expandFirstChild) {
            $('.root', '.pages .tree').find('.arr').first().addClass('expanded');
            $('.root', '.pages .tree').find('.ul').first().show();

            $('.root ul li ul', '.pages .tree').hide();
        } else {
            $('.arr', '.pages .tree').addClass('expanded');
            $('ul', '.pages .tree').show();
        }
    };

    function expandParent(Id) {
        var parentUl = $('#' + Id).parent('li').parent('ul');

        if (parentUl.length > 0) {
            var parentId = $('#' + Id).parent('li').parent('ul').parent('li').find('.item').first().attr('id');

            if (parentId > 0) {
                $('.arr', '#' + parentId).first().addClass('expanded');
                expandParent(parentId);
            }

            parentUl.show();
        }
    };

    function contractTree() {
        $('.arr', '.pages .tree').removeClass('expanded');
        $('ul', '.pages .tree').hide();
    };


    /* AJAX FUNCTIONS
    ***************************************************************************************************************/

    function buildTree() {
        $.ajax({
            url: pageSettings.ajaxMethod,
            type: 'POST',
            dataType: 'text json',
            contentType: 'application/json; charset=utf-8',
            success: function (o) {
                if (o.success) {
                    if (o.tree != '') {
                        treeRoot
                            .append(o.tree);

                        if (pageSettings.useUi) {
                            $(".edit-page").button({
                                icons: {
                                    primary: "ui-icon-pencil"
                                },
                                text: false
                            });
                            $(".delete-page").button({
                                icons: {
                                    primary: "ui-icon-trash"
                                },
                                text: false
                            });
                        }

                        if (pageSettings.selectable) {
                            $('.tick', treeRoot).show('fast');
                        }

                        if (pageSettings.expanded) {
                            expandTree();
                        } else {
                            contractTree();
                        }

                        if (pageSettings.selectedPage != null) {
                            var selectedPage = (pageSettings.selectedPage > 0) ? pageSettings.selectedPage : $('ul.tree').attr('id');

                            $('div#' + selectedPage, '.tree').addClass('active');
                            $('div#' + selectedPage, '.tree').parent().find('.tick').first().css('background-position', '0px -28px')

                            expandParent(selectedPage);
                        }
                    } else {
                        showAnimatedStatusText($(pageSettings.updateElement), 'You currently have no pages.', 'warning');
                    }
                } else {
                    showAnimatedStatusText($(pageSettings.updateElement), o.error, 'error');
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                showAnimatedStatusText($(pageSettings.updateElement), 'An uncaught error has occurred, please contact your system administrator (Error code: 107).', 'error');
            }
        });
    };
};