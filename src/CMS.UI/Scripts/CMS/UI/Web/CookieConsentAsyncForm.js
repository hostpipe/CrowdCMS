$('form[data-async=true]').on('submit', function (event) {
    var $form = $(this);
    var $target = $($form.attr('data-target'));

    $.ajax({
        type: $form.attr('method'),
        url: $form.attr('action'),
        data: $form.serialize(),

        success: function (data, status) {
            $target.html(data);
        }
    });

    event.preventDefault();
});