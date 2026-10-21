mergeInto(LibraryManager.library, {
    
    // 1. Grab the URL parameter on first load
    GetURLParameter: function(paramName) {
        var paramStr = UTF8ToString(paramName);
        var urlParams = new URLSearchParams(window.location.search);
        var paramValue = urlParams.get(paramStr);
        
        if (paramValue) {
            var bufferSize = lengthBytesUTF8(paramValue) + 1;
            var buffer = _malloc(bufferSize);
            stringToUTF8(paramValue, buffer, bufferSize);
            return buffer;
        } else {
            return null;
        }
    },

    // 2. The portal listener
    StartPortalListener: function(gameObjectPtr, callbackMethodPtr) {
        var gameObjName = UTF8ToString(gameObjectPtr);
        var callbackName = UTF8ToString(callbackMethodPtr);

        if (window.portalListenerActive) return;
        window.portalListenerActive = true;

        console.log("GAME DEBUG: Listener successfully attached. Waiting for messages targeting GameObject: '" + gameObjName + "'");

        window.addEventListener("message", function(event) {
            console.log("GAME DEBUG: Message received from parent window. Data payload:", event.data);

            if (event.data && event.data.type === "RESTART_GAME") {
                console.log("GAME DEBUG: RESTART_GAME command matched! Instantly firing RESTART_ACK back to portal...");
                var newSessionId = event.data.sessionId;
                
                window.parent.postMessage({ type: "RESTART_ACK" }, "*");
                console.log("GAME DEBUG: Triggering Unity C# script... Target: '" + gameObjName + "' | Method: '" + callbackName + "' | ID: " + newSessionId);
                
                SendMessage(gameObjName, callbackName, newSessionId);
            }
        });
    },

    // 3. NEW: The missing function causing the build error!
    NotifySessionUpdated: function (newSessionId) {
        var sessionIdStr = UTF8ToString(newSessionId);
        
        // Fire the exact payload Eniibukun requested to the parent Next.js window
        window.parent.postMessage({ 
            type: "SESSION_UPDATED", 
            sessionId: sessionIdStr 
        }, "*");
    },

    // 4. Fast restart: ask the portal for a new session over postMessage
    // instead of the slower authenticated GET /game/session REST call.
    // The portal replies with RESTART_GAME, which StartPortalListener above
    // already listens for and forwards into Unity.
    RequestRestartFromGame: function () {
        window.parent.postMessage({ type: "REQUEST_RESTART" }, "*");
    }
});