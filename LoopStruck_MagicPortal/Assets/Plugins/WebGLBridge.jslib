mergeInto(LibraryManager.library, {

    // 1. Grab URL parameters dynamically (e.g. sessionId)
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

    // 2. Attach the portal event listener for RESTART_GAME messages
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
                
                // Send acknowledgment back to parent iframe
                window.parent.postMessage({ type: "RESTART_ACK" }, "*");
                console.log("GAME DEBUG: Triggering Unity C# script... Target: '" + gameObjName + "' | Method: '" + callbackName + "' | ID: " + newSessionId);
                
                // Dispatch event to Unity C# receiver
                SendMessage(gameObjName, callbackName, newSessionId);
            }
        });
    },

    // 3. Broadcast session updates to Next.js portal
    NotifySessionUpdated: function(newSessionId) {
        var sessionIdStr = UTF8ToString(newSessionId);
        
        window.parent.postMessage({ 
            type: "SESSION_UPDATED", 
            sessionId: sessionIdStr 
        }, "*");
    },

    // 4. Send real-time score updates to parent portal
    PortalSendScoreUpdate: function(score) {
        window.parent.postMessage({
            type: "SCORE_UPDATE",
            score: score
        }, "*");
    },

    // 5. Send match completion message to parent portal
    PortalSendMatchComplete: function(score) {
        window.parent.postMessage({
            type: "MATCH_COMPLETE",
            score: score
        }, "*");
    },

    // 6. Request new game session from parent portal
    PortalRequestRestart: function() {
        window.parent.postMessage({ 
            type: "REQUEST_RESTART" 
        }, "*");
    }
});