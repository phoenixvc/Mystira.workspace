// SSE Client for Blazor WASM
// Uses browser's native EventSource API for Server-Sent Events

window.sseClient = {
    eventSources: {},
    dotNetRefs: {},

    connect: function (sessionId, url, dotNetRef) {
        console.log(`[SSE-JS] Connecting to session ${sessionId} at ${url}`);
        
        // Close existing connection if any
        if (this.eventSources[sessionId]) {
            console.log(`[SSE-JS] Closing existing connection for session ${sessionId} before reconnecting`);
            this.eventSources[sessionId].close();
            delete this.eventSources[sessionId];
        }

        const eventSource = new EventSource(url);
        this.eventSources[sessionId] = eventSource;
        this.dotNetRefs[sessionId] = dotNetRef;

        // Handle all event types
        const eventTypes = [
            'PhaseStarted',
            'GenerationComplete',
            'ValidationFailed',
            'EvaluationPassed',
            'EvaluationFailed',
            'RefinementComplete',
            'RubricGenerated',
            'MaxIterationsReached',
            'Error',
            'TokenUsageUpdate',
            'StreamingUpdate'
        ];

        eventTypes.forEach(eventType => {
            eventSource.addEventListener(eventType, (event) => {
                console.log(`[SSE-JS] Received event: ${eventType}`);
                try {
                    const data = JSON.parse(event.data);
                    console.log(`[SSE-JS] Event data:`, data);
                    
                    // Call back to .NET
                    dotNetRef.invokeMethodAsync('OnSseEvent', eventType, event.data)
                        .catch(err => console.error(`[SSE-JS] Error invoking .NET method:`, err));
                } catch (err) {
                    console.error(`[SSE-JS] Error parsing event data:`, err);
                }
            });
        });

        eventSource.onopen = () => {
            console.log(`[SSE-JS] Connection opened for session ${sessionId}`);
        };

        eventSource.onerror = (error) => {
            console.error(`[SSE-JS] Connection error for session ${sessionId}:`, error);
            
            // Notify .NET of error
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnSseError', error.toString())
                    .catch(err => console.error(`[SSE-JS] Error invoking .NET error handler:`, err));
            }

            // EventSource will automatically try to reconnect
            // If you want to stop reconnection attempts:
            // eventSource.close();
        };

        return true;
    },

    disconnect: function (sessionId) {
        console.log(`[SSE-JS] Disconnecting session ${sessionId}`);
        
        if (this.eventSources[sessionId]) {
            this.eventSources[sessionId].close();
            delete this.eventSources[sessionId];
        }
        
        if (this.dotNetRefs[sessionId]) {
            delete this.dotNetRefs[sessionId];
        }
        
        return true;
    },

    disconnectAll: function () {
        console.log(`[SSE-JS] Disconnecting all sessions`);
        
        Object.keys(this.eventSources).forEach(sessionId => {
            this.disconnect(sessionId);
        });
        
        return true;
    }
};
