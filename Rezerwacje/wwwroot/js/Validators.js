document.addEventListener("DOMContentLoaded", function () {
    if (!window.jQuery || !jQuery.validator || !jQuery.validator.unobtrusive) return;

    // Globalne komunikaty jQuery Validate
    jQuery.extend(jQuery.validator.messages, {
        required: "To pole jest wymagane.",
        email: "Podaj poprawny adres e-mail.",
        equalTo: "Hasła muszą być takie same.",
        minlength: jQuery.validator.format("Wpisz co najmniej {0} znaków."),
        maxlength: jQuery.validator.format("Wpisz maksymalnie {0} znaków."),
        rangelength: jQuery.validator.format("Wpisz od {0} do {1} znaków.")
    });

    // Helper do labelki
    function labelText(el) {
        const id = el.getAttribute("id");
        const lbl = id ? document.querySelector(`label[for="${id}"]`) : null;
        return (lbl ? lbl.textContent.trim() : (el.getAttribute("name") || "Pole"));
    }

    // Podmiana data-val-* na PL
    document.querySelectorAll("input[data-val='true'], select[data-val='true'], textarea[data-val='true']")
        .forEach(el => {
            const name = labelText(el);

            if (el.hasAttribute("data-val-required")) {
                el.setAttribute("data-val-required", `Pole „${name}” jest wymagane.`);
            }
            if (el.hasAttribute("data-val-email")) {
                el.setAttribute("data-val-email", "Podaj poprawny adres e-mail.");
            }
            if (el.hasAttribute("data-val-length-min") || el.hasAttribute("data-val-length-max")) {
                const min = el.getAttribute("data-val-length-min") || "0";
                const max = el.getAttribute("data-val-length-max") || "∞";
                el.setAttribute("data-val-length", `„${name}” musi mieć od ${min} do ${max} znaków.`);
            }
            if (el.hasAttribute("data-val-equalto")) {
                el.setAttribute("data-val-equalto", "Hasła muszą być takie same.");
            }
        });

    // Natychmiastowa walidacja hasła
    jQuery.validator.addMethod("passwordcomplex", function (value, element) {
        if (!value) return true;
        const hasUpper = /[A-Z]/.test(value);
        const hasDigit = /[0-9]/.test(value);
        const hasSpecial = /[^a-zA-Z0-9]/.test(value);
        return hasUpper && hasDigit && hasSpecial;
    }, function (params, element) {
        return "Hasło musi zawierać: 1 wielką literę, 1 cyfrę oraz 1 znak specjalny.";
    });

    //  Podpięcie reguły do pól hasła
    document.querySelectorAll("input[type='password']").forEach(el => {
        const n = (el.getAttribute("name") || "").toLowerCase();
        const id = (el.getAttribute("id") || "").toLowerCase();

        const isPasswordField =
            id.includes("password") ||
            n.includes("password");

        if (!isPasswordField) return;

        el.setAttribute("data-val", "true");
        el.setAttribute("data-val-passwordcomplex",
            "Hasło musi zawierać: 1 wielką literę, 1 cyfrę oraz 1 znak specjalny.");
    });

    // Mapowanie data-val-passwordcomplex do metoda passwordcomplex
    jQuery.validator.unobtrusive.adapters.addBool("passwordcomplex");

    // Parse unobtrusive po zmianach
    jQuery.validator.unobtrusive.parse(document);

    // Walidacja natychmiastowa, disable submit, kursor itp.
    jQuery("form").each(function () {
        const $form = jQuery(this);
        const validator = $form.data("validator");
        if (!validator) return;

        const touched = new Set();

        function isFormValidSilently() {
            validator.checkForm();
            return validator.valid();
        }

        function updateSubmitButton() {
            const $btn = $form.find(":submit").first();
            if (!$btn.length) return;

            const ok = isFormValidSilently();
            $btn.prop("disabled", !ok);
            $btn.toggleClass("btn-disabled", !ok);

            $btn.css("cursor", ok ? "pointer" : "not-allowed");
        }

        updateSubmitButton();

        $form.on("focusout", "input, select, textarea", function () {
            if (!this.name) return;
            touched.add(this.name);
            jQuery(this).valid();
            updateSubmitButton();
        });

        $form.on("keyup change", "input, select, textarea", function () {
            if (!this.name) return;
            if (!touched.has(this.name)) {
                updateSubmitButton();
                return;
            }
            jQuery(this).valid();
            updateSubmitButton();
        });

        $form.on("submit", function () {
            $form.find("input, select, textarea").each(function () {
                if (this.name) touched.add(this.name);
            });
            updateSubmitButton();
        });
    });
});
