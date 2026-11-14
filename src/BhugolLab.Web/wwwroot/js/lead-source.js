// Copies UTM parameters and the document referrer into the hidden lead-source
// fields of the contact form. The only JavaScript on the site (~400 bytes).
(function () {
  "use strict";
  var params = new URLSearchParams(window.location.search);
  var values = {
    utm_source: params.get("utm_source") || "",
    utm_medium: params.get("utm_medium") || "",
    utm_campaign: params.get("utm_campaign") || "",
    referrer: document.referrer || ""
  };
  Object.keys(values).forEach(function (key) {
    var inputs = document.querySelectorAll('input[data-utm="' + key + '"]');
    for (var i = 0; i < inputs.length; i++) {
      inputs[i].value = values[key];
    }
  });
})();
