// Auto-dismisses flash messages after 7 seconds unless the user is hovering or focused on them.
function initFlashMessages() {
    document.querySelectorAll("[data-flash-message]").forEach((message) => {
        // Enhanced navigation can re-run this over elements that are already set up.
        if (message.dataset.flashReady) {
            return;
        }
        message.dataset.flashReady = "true";

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
}

initFlashMessages();

// Blazor enhanced navigation swaps page content without a full reload.
if (window.Blazor) {
    window.Blazor.addEventListener("enhancedload", initFlashMessages);
}
