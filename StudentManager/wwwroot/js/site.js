// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.querySelectorAll("[data-flash-message]").forEach((message) => {
    let dismissTimer;

    const scheduleDismiss = () => {
        window.clearTimeout(dismissTimer);

        if (!message.matches(":hover") && !message.contains(document.activeElement)) {
            dismissTimer = window.setTimeout(() => {
                bootstrap.Alert.getOrCreateInstance(message).close();
            }, 7000);
        }
    };

    message.addEventListener("mouseenter", () => window.clearTimeout(dismissTimer));
    message.addEventListener("mouseleave", scheduleDismiss);
    message.addEventListener("focusin", () => window.clearTimeout(dismissTimer));
    message.addEventListener("focusout", (event) => {
        if (!message.contains(event.relatedTarget)) {
            scheduleDismiss();
        }
    });

    scheduleDismiss();
});
