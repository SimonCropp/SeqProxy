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

// begin-snippet: StructuredLogConfigExtraProp
const logWithExtraProps = structuredLog.configure()
    .filter(logEvent => {
        const template = logEvent.messageTemplate;
        template.raw = template.raw.replace('{@Properties}','');
        return true;
    })
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

function LogInputStructuredWithExtraProps() {
    LogStructuredWithExtraProps(document.getElementById('textInput').value);
}

// begin-snippet: StructuredLogWithExtraProps
function LogStructuredWithExtraProps(text) {
    logWithExtraProps.info(
        'StructuredLog input: {Text} {@Properties}',
        text,
        {
            Timezone: new Date().getTimezoneOffset(),
            Language: navigator.language
        });
}
// end-snippet

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
        body: `{'@mt':'RawJs input: {Text}','Text':'${text}'}`
    };

    return fetch('/api/events/raw', postSettings);
}
// end-snippet