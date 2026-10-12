// Delegated listeners keep the drop dialogs working after Blazor enhanced navigation replaces the DOM.
document.addEventListener("click", (event) => {
    const dropButton = event.target.closest(".course-drop-button");
    if (dropButton) {
        document.getElementById(dropButton.dataset.dialogId)?.showModal();
        return;
    }

    const cancelButton = event.target.closest(".cancel-dialog");
    if (cancelButton) {
        cancelButton.closest("dialog")?.close();
    }
});
