/**
 * JobPortPro - Client-side Interactive Engine & SweetAlert2 Integration
 */

// Custom Toast Notification Configuration
const SwalToast = typeof Swal !== 'undefined' ? Swal.mixin({
    toast: true,
    position: 'top-end',
    showConfirmButton: false,
    timer: 3500,
    timerProgressBar: true,
    didOpen: (toast) => {
        toast.addEventListener('mouseenter', Swal.stopTimer);
        toast.addEventListener('mouseleave', Swal.resumeTimer);
    }
}) : null;

// Global Helper Object for SweetAlert Modals and Toasts
window.JobAlert = {
    toast: function (message, icon = 'success') {
        if (SwalToast) {
            SwalToast.fire({
                icon: icon,
                title: message
            });
        } else {
            alert(message);
        }
    },
    success: function (message, title = 'Success!') {
        if (typeof Swal !== 'undefined') {
            return Swal.fire({
                icon: 'success',
                title: title,
                text: message,
                confirmButtonColor: '#2563eb',
                customClass: {
                    popup: 'rounded-4 shadow-lg border-0',
                    confirmButton: 'btn btn-primary rounded-pill px-4'
                }
            });
        } else {
            alert(message);
        }
    },
    error: function (message, title = 'Oops...') {
        if (typeof Swal !== 'undefined') {
            return Swal.fire({
                icon: 'error',
                title: title,
                text: message,
                confirmButtonColor: '#ef4444',
                customClass: {
                    popup: 'rounded-4 shadow-lg border-0',
                    confirmButton: 'btn btn-danger rounded-pill px-4'
                }
            });
        } else {
            alert(message);
        }
    },
    warning: function (message, title = 'Attention') {
        if (typeof Swal !== 'undefined') {
            return Swal.fire({
                icon: 'warning',
                title: title,
                text: message,
                confirmButtonColor: '#f59e0b',
                customClass: {
                    popup: 'rounded-4 shadow-lg border-0',
                    confirmButton: 'btn btn-warning text-dark rounded-pill px-4'
                }
            });
        } else {
            alert(message);
        }
    },
    info: function (message, title = 'Information') {
        if (typeof Swal !== 'undefined') {
            return Swal.fire({
                icon: 'info',
                title: title,
                text: message,
                confirmButtonColor: '#0ea5e9',
                customClass: {
                    popup: 'rounded-4 shadow-lg border-0',
                    confirmButton: 'btn btn-info text-white rounded-pill px-4'
                }
            });
        } else {
            alert(message);
        }
    },
    confirm: function (options) {
        if (typeof Swal !== 'undefined') {
            return Swal.fire({
                title: options.title || 'Are you sure?',
                text: options.text || "You won't be able to revert this!",
                icon: options.icon || 'warning',
                showCancelButton: true,
                confirmButtonColor: options.confirmButtonColor || '#ef4444',
                cancelButtonColor: '#64748b',
                confirmButtonText: options.confirmButtonText || 'Yes, proceed',
                cancelButtonText: options.cancelButtonText || 'Cancel',
                reverseButtons: true,
                customClass: {
                    popup: 'rounded-4 shadow-lg border-0',
                    confirmButton: 'btn btn-danger rounded-pill px-4 ms-2',
                    cancelButton: 'btn btn-secondary rounded-pill px-4'
                },
                buttonsStyling: false
            });
        } else {
            return Promise.resolve({ isConfirmed: confirm(options.text || options.title) });
        }
    }
};

// Automatic Event Listener for SweetAlert Confirm on Forms and Buttons
document.addEventListener('DOMContentLoaded', function () {
    // Intercept forms with data-swal-confirm attribute
    document.querySelectorAll('form[data-swal-confirm]').forEach(form => {
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            const message = form.getAttribute('data-swal-confirm') || 'Are you sure you want to proceed?';
            const title = form.getAttribute('data-swal-title') || 'Confirm Action';
            const btnText = form.getAttribute('data-swal-btn') || 'Yes, Confirm';
            const icon = form.getAttribute('data-swal-icon') || 'warning';

            JobAlert.confirm({
                title: title,
                text: message,
                confirmButtonText: btnText,
                icon: icon
            }).then((result) => {
                if (result.isConfirmed) {
                    form.removeAttribute('data-swal-confirm');
                    form.submit();
                }
            });
        });
    });

    // Intercept anchor tags with data-swal-confirm
    document.querySelectorAll('a[data-swal-confirm]').forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();
            const href = link.getAttribute('href');
            const message = link.getAttribute('data-swal-confirm') || 'Are you sure you want to proceed?';
            const title = link.getAttribute('data-swal-title') || 'Confirm Action';
            const btnText = link.getAttribute('data-swal-btn') || 'Yes, Proceed';

            JobAlert.confirm({
                title: title,
                text: message,
                confirmButtonText: btnText
            }).then((result) => {
                if (result.isConfirmed && href && href !== '#') {
                    window.location.href = href;
                }
            });
        });
    });
});
