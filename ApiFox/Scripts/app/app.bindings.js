// Bind Bootstrap Popover
ko.bindingHandlers.popover = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var $element = $(element);
        var popoverBindingValues = ko.utils.unwrapObservable(valueAccessor());
        var template = popoverBindingValues.template || false;
        var options = popoverBindingValues.options || { title: 'popover' };
        var data = popoverBindingValues.data || false;
        if (template !== false) {
            if (data) {
                options.content = "<!-- ko template: { name: template, if: data, data: data } --><!-- /ko -->";
            }
            else {
                options.content = $('#' + template).html();
            }
            options.html = true;
        }
        $element.on('shown.bs.popover', function (event) {

            var popoverData = $(event.target).data();
            var popoverEl = popoverData['bs.popover'].$tip;
            var options = popoverData['bs.popover'].options || {};
            var button = $(event.target);
            var buttonPosition = button.position();
            var buttonDimensions = {
                x: button.outerWidth(),
                y: button.outerHeight()
            };

            ko.cleanNode(popoverEl[0]);
            if (data) {
                ko.applyBindings({ template: template, data: data }, popoverEl[0]);
            }
            else {
                ko.applyBindings(viewModel, popoverEl[0]);
            }

            var popoverDimensions = {
                x: popoverEl.outerWidth(),
                y: popoverEl.outerHeight()
            };

            popoverEl.find('button[data-dismiss="popover"]').click(function () {
                button.popover('hide');
            });

            switch (options.placement) {
                case 'right':
                    popoverEl.css({
                        left: buttonDimensions.x + buttonPosition.left,
                        top: (buttonDimensions.y / 2 + buttonPosition.top) - popoverDimensions.y / 2
                    });
                    break;
                case 'left':
                    popoverEl.css({
                        left: buttonPosition.left - popoverDimensions.x,
                        top: (buttonDimensions.y / 2 + buttonPosition.top) - popoverDimensions.y / 2
                    });
                    break;
                case 'top':
                    popoverEl.css({
                        left: buttonPosition.left + (buttonDimensions.x / 2 - popoverDimensions.x / 2),
                        top: buttonPosition.top - popoverDimensions.y
                    });
                    break;
                case 'bottom':
                    popoverEl.css({
                        left: buttonPosition.left + (buttonDimensions.x / 2 - popoverDimensions.x / 2),
                        top: buttonPosition.top + buttonDimensions.y
                    });
                    break;
            }
        });

        $element.popover(options);
        ko.utils.domNodeDisposal.addDisposeCallback(element, function () {
            $element.popover('destroy');
        });

        return { controlsDescendantBindings: true };

    }
};
 
ko.bindingHandlers.kendoupload = {
    init: function (element, valueAccessor, allBindingAccessor, viewModel, bindingContext) {
        var $element = $(element);
        var options = ko.utils.unwrapObservable(valueAccessor()) || {};
        $element.kendoUpload({
            async: {
                saveUrl: "Home/Save",
                removeUrl: "Home/Remove",
                autoUpload: true
            },
            success: options.onSuccess,
            cancel: onCancel,
            complete: onComplete,
            error: onError,
            progress: onProgress,
            remove: onRemove,
            select: onSelect,
            upload: onUpload
        });

        function onSelect(e) {
            console.log("Select :: " + getFileInfo(e));
        }

        function onUpload(e) {
            console.log("Upload :: " + getFileInfo(e));
        }

        function onSuccess(e) {
            console.log("Success (" + e.operation + ") :: " + getFileInfo(e));
        }

        function onError(e) {
            console.log("Error (" + e.operation + ") :: " + getFileInfo(e));
        }

        function onComplete(e) {
            console.log("Complete");
        }

        function onCancel(e) {
            console.log("Cancel :: " + getFileInfo(e));
        }

        function onRemove(e) {
            console.log("Remove :: " + getFileInfo(e));
        }

        function onProgress(e) {
            console.log("Upload progress :: " + e.percentComplete + "% :: " + getFileInfo(e));
        }
        function getFileInfo(e) {
            return $.map(e.files, function (file) {
                var info = file.name;

                // File size is not available in all browsers
                if (file.size > 0) {
                    info += " (" + Math.ceil(file.size / 1024) + " KB)";
                }
                return info;
            }).join(", ");
        }
    }
};
