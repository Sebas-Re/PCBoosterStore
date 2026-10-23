/* Comportamientos del markup declarados con atributos data-*, para no usar
   JavaScript inline (la Content-Security-Policy no lo permite):
   - <img data-respaldo>: si la imagen no carga, muestra la imagen de reemplazo.
   - <form data-confirmar="¿Seguro?">: pide confirmación antes de enviar.
   - <select data-autoenviar>: envía el formulario al elegir una opción. */
(function () {
    "use strict";

    var respaldo = document.body.getAttribute("data-imagen-respaldo");

    function reemplazar(img) {
        // Una sola vez: si también fallara el reemplazo, no queda en un bucle.
        if (img.hasAttribute("data-respaldo") && !img.hasAttribute("data-reemplazada")) {
            img.setAttribute("data-reemplazada", "");
            img.src = respaldo;
        }
    }

    // Los errores de carga no burbujean: se escuchan en la fase de captura.
    document.addEventListener("error", function (e) {
        if (e.target instanceof HTMLImageElement) {
            reemplazar(e.target);
        }
    }, true);

    // Imágenes que fallaron antes de que este script cargara.
    document.querySelectorAll("img[data-respaldo]").forEach(function (img) {
        if (img.complete && img.naturalWidth === 0) {
            reemplazar(img);
        }
    });

    // <select data-autoenviar>: envía su formulario al cambiar la opción.
    document.addEventListener("change", function (e) {
        if (e.target.matches("select[data-autoenviar]") && e.target.form) {
            e.target.form.requestSubmit();
        }
    });

    document.addEventListener("submit", function (e) {
        var mensaje = e.target.getAttribute("data-confirmar");
        if (mensaje && !window.confirm(mensaje)) {
            e.preventDefault();
        }
    });
})();
