$(() => {
    new NotificationSettingsEditor();
});

class NotificationSettingsEditor {
    constructor() {
        let $all = $('input[name="enabledTypeMethods"]');

        $all.on('change', (ev) => {
            setTimeout(() => {
                let notifType = $(ev.target).attr('data-notif-type');
                let notifMethod = $(ev.target).attr('data-notif-method');
                if ($all.filter('[data-notif-type="' + notifType + '"]:checked').length > 0) {
                    if (notifMethod === 'None') {
                        $all.filter('[data-notif-type="' + notifType + '"][data-notif-method!="None"]').prop('checked', false);
                    } else {
                        $all.filter('[data-notif-type="' + notifType + '"][data-notif-method="None"]').prop('checked', false);
                    }
                } else {
                    $all.filter('[data-notif-type="' + notifType + '"][data-notif-method="None"]').prop('checked', true);
                }
            }, 250);
        });
    }
}