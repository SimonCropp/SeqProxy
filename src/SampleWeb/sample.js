// begin-snippet: StructuredLogConfig
var levelSwitch = new structuredLog.DynamicLevelSwitch('info');
const log = structuredLog.configure()
    .writeTo(new structuredLog.ConsoleSink())
    .minLevel(levelSwitch)
    .writeTo(SeqSink({
        url: `${location.protocol}//${location.host}`,
        compact: true,
        levelSwitch: levelSwitch
    }))
    .create();
// end-snippet

function LogInputStructured() {
    LogStructured(document.getElementById('textInput').value);
}

// begin-snippet: StructuredLog
function LogStructured(text) {
    log.info('StructuredLog input: {Text}', text);
}
// end-snippet

function LogInputRawJs() {
    return LogRawJs(document.getElementById('textInput').value);
}
// begin-snippet: LogRawJs
function LogRawJs(text) {
    const postSettings = {
        method: 'POST',
        credentials: 'include',
        mode: 'cors',
        body: `{'@mt':'RawJs input: {Text}','Text':'${text}'}`
    };

    return fetch('/api/events/raw', postSettings);
}
// end-snippet