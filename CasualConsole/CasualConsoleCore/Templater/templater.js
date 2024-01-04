const Templater = function () {
    function err() {
        throw new Error();
    }
    function htmlEncode(s) {
        s = String(s ?? "");
        return s.replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/'/g, '&#39;')
            .replace(/"/g, '&#34;');
    }
    function compile(template) {
        const handlers = [];
        let end = 0;
        let handler;
        while (end < template.length) {
            [handler, end] = getHandler(template, end);
            if (!handler)
                err();
            handlers.push(handler);
        }
        return (data, helpers) => {
            helpers ??= {};
            const parts = [];
            const writer = x => parts.push(x);
            const contextData = {};
            contextData["$"] = data;
            const context = {
                get(key) {
                    return contextData[key] ?? helpers[key] ?? data[key];
                },
                set(name, val) {
                    contextData[name] = val;
                }
            };
            handleMulti(writer, context, handlers);
            return parts.join("");
        };
    }
    function getHandler(template, start) {
        if (start == template.length) err();
        const i = template.indexOf("{{", start);
        if (i == start) {
            let [tokens, end] = getTokens(template, i + 2);
            const first = tokens[0];
            if (first === "if") {
                const ifExpr = getExpression(tokens, 1);
                let ifHandlers;
                [ifHandlers, end] = getBodyHandlers(template, end);
                let nextHandlerType = getHandlerType(template, end);
                if (nextHandlerType !== "else") {
                    const simpleHandler = (writer, context) => {
                        if (ifExpr(context))
                            handleMulti(writer, context, ifHandlers);
                    };
                    return [simpleHandler, end];
                }
                const elseIfHandlers = [];
                let elseHandlers = null;
                let elseTokens;
                while (true) {
                    [elseTokens, end] = getTokens(template, end + 2);
                    if (elseTokens.length == 1) {
                        [elseHandlers, end] = getBodyHandlers(template, end);
                        break;
                    }
                    else {
                        if (elseTokens[1] !== "if") err();
                        const elseIfExpr = getExpression(elseTokens, 2);
                        let elseIfInnerHandlers;
                        [elseIfInnerHandlers, end] = getBodyHandlers(template, end);
                        elseIfHandlers.push([elseIfExpr, elseIfInnerHandlers]);
                    }
                    nextHandlerType = getHandlerType(template, end);
                    if (nextHandlerType !== "else")
                        break;
                }
                const handler = (writer, context) => {
                    if (ifExpr(context)) {
                        handleMulti(writer, context, ifHandlers);
                        return;
                    }
                    for (const [elseIfExpr, elseIfHand] of elseIfHandlers) {
                        if (elseIfExpr(context)) {
                            handleMulti(writer, context, elseIfHand);
                            return;
                        }
                    }
                    if (elseHandlers) {
                        handleMulti(writer, context, elseHandlers);
                        return;
                    }
                }
                return [handler, end];
            }
            else if (first === "for") {
                if (tokens[2] !== "in") {
                    const inIndex = tokens.indexOf("in");
                    if (inIndex > 2)
                        tokens = mergeTokens(tokens, inIndex);
                    else
                        err();
                }
                const [loopVarName, loopIndexName] = tokens[1].split(",");
                const loopValuesExpr = getExpression(tokens, 3);
                let handlers;
                [handlers, end] = getBodyHandlers(template, end);
                const handler = (writer, context) => {
                    const loopValues = loopValuesExpr(context);
                    let i = 0;
                    for (const val of loopValues) {
                        context.set(loopVarName, val);
                        if (loopIndexName)
                            context.set(loopIndexName, i);
                        handleMulti(writer, context, handlers);
                        i++;
                    }
                }
                return [handler, end];
            }
            else if (first === "end" || first === "else") {
                return [null, end];
            }
            else {
                const expr = getExpression(tokens, 0);
                const handler = (writer, context) => {
                    writer(htmlEncode(expr(context)));
                }
                return [handler, end];
            }
        }
        else if (i < 0) {
            return [template.substring(start), template.length];
        }
        else {
            return [template.substring(start, i), i];
        }
    }
    function mergeTokens(tokens, inIndex) {
        const merged = tokens.slice(1, inIndex).join("");
        return [tokens[0], merged, ...tokens.slice(inIndex)];
    }
    function handleMulti(writer, context, handlers) {
        for (const h of handlers)
            if (typeof h === "string")
                writer(h);
            else
                h(writer, context);
    }
    function getTokens(template, i) {
        const tokens = [];
        while (true) {
            while (template[i] === ' ')
                i++;
            if (template[i] === '}' && template[i + 1] === '}')
                return [tokens, i + 2];
            if (template[i] === '.') {
                tokens.push(".");
                i++;
                continue;
            }
            const start = i++;
            while (i < template.length && template[i] !== '.' && template[i] !== '}' && template[i] !== ' ')
                i++;
            const token = template.substring(start, i);
            if (!token)
                err();
            tokens.push(token);
        }
    }
    function getHandlerType(template, start) {
        while (start < template.length && /\s/.test(template[start])) // checking for whitespace
            start++;
        if (start + 1 < template.length && template[start] === '{' && template[start + 1] === '{') {
            start += 2;
            while (template[start] === ' ')
                start++;
            const tempStart = start++;
            while (template[start] !== '.' && template[start] !== '}' && template[start] !== ' ')
                start++;
            return template.substring(tempStart, start);
        }
        return null;
    }
    function getBodyHandlers(template, end) {
        const handlers = [];
        let handler;
        while (true) {
            [handler, end] = getHandler(template, end);
            if (!handler)
                break;
            handlers.push(handler);
        }
        return [handlers, end];
    }
    function getExpression(tokens, start) {
        if (tokens.length - start == 0 || tokens[start] === '.')
            err();
        if (tokens.length - start == 1)
            return getTokenAsExpression(tokens[start]);
        if (tokens[start + 1] === ".") {
            const argGroups = getArgGroups(tokens, start);
            if (argGroups.length == 1)
                return argGroups[0];
            err();
        }
        else {
            const f = tokens[start];
            const argGroups = getArgGroups(tokens, start + 1);
            return context => {
                const func = context.get(f);
                if (typeof func !== "function")
                    throw new Error(`value of ${f} was not a function`);
                const args = argGroups.map(expr => expr(context));
                return func.apply(null, args);
            }
        }
    }
    function getTokenAsExpression(token) {
        return context => context.get(token);
    }
    function getMemberAccessExpression(tokens, start, end) {
        end -= start;
        if (end == 1) {
            return getTokenAsExpression(tokens[start]);
        }
        else if (end == 3) {
            const objName = tokens[start];
            const key = tokens[start + 2];
            return context => {
                return context.get(objName)[key];
            };
        }
        else if (end == 5) {
            const objName = tokens[start];
            const key1 = tokens[start + 2];
            const key2 = tokens[start + 4];
            return context => {
                return context.get(objName)[key1][key2];
            };
        }
        err();
    }
    function getArgGroups(tokens, start) {
        const args = [];
        while (true) {
            let end = start;
            if (tokens[end] === '.') err();
            end++;
            while (end < tokens.length && tokens[end] === '.') {
                end++;
                if (tokens[end] === '.' || tokens[end] === undefined)
                    err();
                end++;
            }

            args.push(getMemberAccessExpression(tokens, start, end));
            if (end == tokens.length)
                return args;
            start = end;
        }
    }
    return { compile };
}();
export default Templater;