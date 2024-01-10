const Templater = (() => {
    const registeredHelpers = {};
    const compile = template => {
        const handlers = [];
        let end = 0;
        let handler;
        while (end < template.length) {
            [handler, end] = getHandler(template, end);
            if (!handler)
                throw new Error("Unexpected end token");
            handlers.push(handler);
        }
        return (data, helpers = {}) => {
            const parts = [];
            const writer = x => parts.push(x);
            const contextData = {};
            contextData["$"] = data;
            const context = {
                get(key) {
                    return contextData[key] ?? helpers[key] ?? registeredHelpers[key] ?? data[key];
                },
                set(name, val) {
                    contextData[name] = val;
                }
            };
            handleMulti(writer, context, handlers);
            return parts.join("");
        };
    };
    const registerHelper = (name, f) => registeredHelpers[name] = f;
    const getHandler = (template, start) => {
        if (start == template.length) throw new Error("Expected {{end}} but not found");
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
                        if (elseTokens[1] !== "if") throw new Error("Unexpected expression after else");
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
                };
                return [handler, end];
            }
            else if (first === "for") {
                let loopType = tokens[2];
                if (loopType !== "in" && loopType !== "range") {
                    const inIndex = tokens.findIndex(x => x === "in" || x === "range");
                    if (inIndex > 2) {
                        tokens = mergeTokens(tokens, inIndex);
                        loopType = tokens[2];
                    }
                    else
                        throw new Error("For loops have to be either 'in' or 'range' loops");
                }
                const [t1, t2] = tokens[1].split(",");
                const loopValuesExpr = getExpression(tokens, 3);
                let handlers;
                [handlers, end] = getBodyHandlers(template, end);
                const handler = loopType === "in"
                    ? (writer, context) => {
                        const loopValues = loopValuesExpr(context);
                        if (loopValues == null) throw new Error(`Value of '${tokens.slice(3).join(" ")}' was not iterable`);
                        let i = 0;
                        for (const val of loopValues) {
                            context.set(t1, val);
                            if (t2)
                                context.set(t2, i);
                            handleMulti(writer, context, handlers);
                            i++;
                        }
                    }
                    : (writer, context) => {
                        const obj = loopValuesExpr(context);
                        for (const key in obj) {
                            context.set(t1, key);
                            context.set(t2, obj[key]);
                            handleMulti(writer, context, handlers);
                        }
                    };
                return [handler, end];
            }
            else if (first === "end") {
                return [null, end];
            }
            else if (first === "else") {
                throw new Error("Unexpected else token");
            }
            else if (first === "set") {
                const varName = tokens[1];
                const expr = getExpression(tokens, 2);
                const handler = (writer, context) => context.set(varName, expr(context));
                return [handler, end];
            }
            else {
                const expr = getExpression(tokens, 0);
                const handler = (writer, context) => writer(htmlEncode(expr(context)));
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
    const mergeTokens = (tokens, inIndex) => [tokens[0], tokens.slice(1, inIndex).join(""), ...tokens.slice(inIndex)];
    const handleMulti = (writer, context, handlers) => {
        for (const h of handlers)
            if (typeof h === "string")
                writer(h);
            else
                h(writer, context);
    }
    const getTokens = (template, i) => {
        const tokens = [];
        while (true) {
            while (template[i] === ' ')
                i++;
            if (template[i] === '}' && template[i + 1] === '}')
                return [tokens, i + 2];
            const start = i++;
            while (i < template.length && template[i] !== '}' && template[i] !== ' ')
                i++;
            const token = template.substring(start, i);
            if (!token)
                throw new Error("Tag not closed with }}")
            tokens.push(token);
        }
    }
    const getHandlerType = (template, start) => {
        while (start < template.length && /\s/.test(template[start])) // checking for whitespace
            start++;
        if (start + 1 < template.length && template[start] === '{' && template[start + 1] === '{') {
            start += 2;
            while (template[start] === ' ')
                start++;
            const tempStart = start++;
            while (start < template.length && template[start] !== '}' && template[start] !== ' ')
                start++;
            return template.substring(tempStart, start);
        }
        return null;
    }
    const getBodyHandlers = (template, end) => {
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
    const getExpression = (tokens, start) => {
        if (tokens.length - start == 0 || tokens[start] === '.') {
            if (tokens.length == 0) throw new Error("Expression cannot be empty");
            else throw new Error(`Expression expected after: ${tokens.slice(0, start).join(" ")}`);
        }
        if (tokens.length - start == 1)
            return getTokenAsExpression(tokens[start]);
        const f = tokens[start];
        if (f.includes(".")) throw new Error(`Function name cannot contain a dot character: ${f}`);
        const argGroups = getArgGroups(tokens, start + 1);
        if (argGroups.length == 1) {
            const expr = argGroups[0];
            return context => {
                const func = getFunc(f, context);
                return func(expr(context));
            }
        }
        return context => {
            const func = getFunc(f, context);
            const args = argGroups.map(expr => expr(context));
            return func.apply(null, args);
        }
    }
    const getFunc = (f, context) => {
        const func = context.get(f);
        if (typeof func !== "function")
            throw new Error(`value of ${f} was not a function`);
        return func;
    }
    const getTokenAsExpression = token => {
        const expr = getTokenAsExpressionInner(token);
        return context => {
            try {
                return expr(context);
            } catch (e) {
                throw new Error(`Error while resolving: ${token}`);
            }
        };
    }
    const getTokenAsExpressionInner = token => {
        const subTokens = token.split(".");
        subTokens.forEach(x => { if (!x) throw new Error(`Invalid member access expression: ${token}`) });
        const t = subTokens[0];
        if (subTokens.length == 1) {
            return context => context.get(t);
        }
        if (subTokens.length == 2) {
            const k = subTokens[1];
            return context => context.get(t)[k];
        }
        if (subTokens.length == 3) {
            const k1 = subTokens[1];
            const k2 = subTokens[2];
            return context => context.get(t)[k1][k2];
        }
        return context => {
            let o = context.get(t);
            for (let i = 1; i < subTokens.length; i++)
                o = o[subTokens[i]];
            return o;
        }
    }
    const getArgGroups = (tokens, index) => {
        const exprs = [];
        while (index < tokens.length)
            exprs.push(getTokenAsExpression(tokens[index++]));
        return exprs;
    }
    const htmlEncode = s => {
        s = String(s ?? "");
        return /[&<>\'\"]/.test(s)
            ? s.replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/>/g, '&gt;')
                .replace(/'/g, '&#39;')
                .replace(/"/g, '&#34;')
            : s;
    }
    return { compile, registerHelper };
})();
export default Templater;