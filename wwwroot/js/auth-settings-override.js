// The AuthenticationService provided by Microsoft is used to create the UserManager object. Parameter settings
// can be overridden, and it seems beneficial to do just this rather than to modify the source of the AuthenticationService
// itself. This script will override the UserManager settings to set the silentRequestTimeout and metadataRequestTimeout to 5 seconds.
// 0 seconds seems to a value that is overridden by the AuthenticationService itself, so setting it to 5 seconds should be a good.

(function () {
    console.log("Waiting for AuthenticationService initialization...");

    // Function to modify the UserManager settings
    function modifyAuthSettings() {
        const origCreateUserManagerCore = AuthenticationService.createUserManagerCore;

        AuthenticationService.createUserManagerCore = function (finalSettings) {
            console.log("Intercepting UserManager creation...");

            // Modify the settings before creating UserManager
            const modifiedSettings = {
                ...finalSettings,
                silentRequestTimeout: 5,
                metadataRequestTimeout: 5
            };

            console.log("Modified auth settings:", modifiedSettings);
            return origCreateUserManagerCore.call(this, modifiedSettings);
        };
    }

    // Check if AuthenticationService exists, if not wait for it
    if (window.AuthenticationService) {
        modifyAuthSettings();
    } else {
        Object.defineProperty(window, 'AuthenticationService', {
            configurable: true,
            set: function (service) {
                console.log("AuthenticationService detected");
                delete window.AuthenticationService;
                window.AuthenticationService = service;
                modifyAuthSettings();
            },
            get: function () {
                return undefined;
            }
        });
    }
})();