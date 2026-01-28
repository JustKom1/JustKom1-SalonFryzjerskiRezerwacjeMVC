document.addEventListener("DOMContentLoaded", function () {
    if (!window.jQuery || !jQuery.validator || !jQuery.validator.unobtrusive) return;

    // ✅ 1) Polskie komunikaty jQuery Validate
    jQuery.extend(jQuery.validator.messages, {
        required: "To pole jest wymagane.",
        email: "Podaj poprawny adres e-mail.",
        equalTo: "Hasła muszą być takie same.",
        minlength: jQuery.validator.format("Wpisz co najmniej {0} znaków."),
        maxlength: jQuery.validator.format("Wpisz maksymalnie {0} znaków."),
        rangelength: jQuery.validator.format("Wpisz od {0} do {1} znaków.")
    });

    // ✅ 2) Podmień angielskie data-val-* (ASP.NET często generuje je po angielsku)
    function labelText(el) {
        const id = el.getAttribute("id");
        const lbl = id ? document.querySelector(`label[for="${id}"]`) : null;
        return (lbl ? lbl.textContent.trim() : (el.getAttribute("name") || "Pole"));
    }

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

    // ✅ 3) Przeparsuj walidację po zmianach
    jQuery.validator.unobtrusive.parse(document);

    // ✅ 4) UX: błędy pokazuj dopiero po "dotknięciu" pola (blur)
    // a potem aktualizuj na bieżąco (keyup) tylko dla dotkniętych
    jQuery("form").each(function () {
        const $form = jQuery(this);
        const validator = $form.data("validator");
        if (!validator) return;

        // Kolekcja dotkniętych pól (po name)
        const touched = new Set();

        function isFormValidSilently() {
            // checkForm() nie wyświetla błędów, tylko liczy poprawność
            validator.checkForm();
            return validator.valid();
        }

        function updateSubmitButton() {
            const $btn = $form.find(":submit").first();
            if (!$btn.length) return;

            const ok = isFormValidSilently();
            $btn.prop("disabled", !ok);
            $btn.toggleClass("btn-disabled", !ok);
        }

        // start: przycisk ma być zablokowany na pustym formularzu
        updateSubmitButton();

        // "dotknięcie" pola = blur/focusout
        $form.on("focusout", "input, select, textarea", function () {
            if (!this.name) return;
            touched.add(this.name);
            jQuery(this).valid();   // pokaż błąd tylko dla tego pola
            updateSubmitButton();
        });

        // podczas pisania – waliduj tylko jeśli już było dotknięte
        $form.on("keyup change", "input, select, textarea", function () {
            if (!this.name) return;
            if (!touched.has(this.name)) {
                updateSubmitButton(); // tylko update submit (bez błędów)
                return;
            }
            jQuery(this).valid(); // aktualizuj komunikat dla dotkniętego pola
            updateSubmitButton();
        });

        // jeśli user kliknie submit – wtedy dotykamy wszystkie pola, żeby pokazało błędy
        $form.on("submit", function () {
            $form.find("input, select, textarea").each(function () {
                if (this.name) touched.add(this.name);
            });
            updateSubmitButton();
        });
    });
});
