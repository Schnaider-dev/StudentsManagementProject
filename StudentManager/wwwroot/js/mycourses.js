document.querySelectorAll(".course-drop-button").forEach(button => {
    button.addEventListener("click", () => {
        const dialog = document.getElementById(button.dataset.dialogId);
        dialog.showModal();
    })
})

document.querySelectorAll(".cancel-dialog").forEach(button => {
    button.addEventListener("click", () => {
        button.closest("dialog").close();
    })
})