export function show(dialog) {
    if (dialog instanceof HTMLDialogElement && !dialog.open) {
        dialog.showModal();
    }
}

export function close(dialog) {
    if (dialog instanceof HTMLDialogElement && dialog.open) {
        dialog.close();
    }
}
