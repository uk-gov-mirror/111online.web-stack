import './vendor/jquery.cookie.js'
import './vendor/jquery.validate.js'
import './vendor/jquery.validate.unobtrusive.js'
import './vendor/jquery.unobtrusive-ajax.js'
import './vendor/jquery.ns-autogrow.js'

//global.validation = require('./validation')
global.geolocation = require('./geolocation')


jQuery.validator.setDefaults({
    highlight: function (element, errorClass, validClass) {
        $(element).closest(".form-group").addClass("form-group-error");
    },
    unhighlight: function (element, errorClass, validClass) {
        $(element).closest(".form-group").removeClass("form-group-error");
    }
});