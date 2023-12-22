const Templater = function(){
    function htmlEncode(s){
        if (typeof s !== "string"){
            s = String(s ?? "")
        };
        return s.replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/'/g, '&#39;')
        .replace(/"/g, '&#34;');
    }
    function compile(template){
        const handlers = [];
        let end = 0;
        let handler;
        while (end < template.length){
            [handler, end] = getHandler(template, end);
            handlers.push(handler);
        }
        return (data, helpers) => {
            const parts = [];
            const writer = x => parts.push(x);
            const contextData = {};
            const context = {
                get(key){
                    return contextData[key] ?? helpers[key] ?? data[key];
                },
                set(name, val){
                    contextData[name] = val;
                }
            };
            handleMulti(writer, context, handlers);
            return parts.join("");
        };
    }
    function getHandler(template, start){
        const i = template.indexOf("{{", start);
        if (i == start){
            let [tokens, end] = getTokens(template, i + 2);
            const first = tokens[0];
            if (first === "if"){
                const ifExpr = getExpression(tokens, 1);
                let ifHandlers;
                [ifHandlers, end] = getBodyHandlers(template, end);
                let nextHandlerType = getHandlerType(template, end);
                if (nextHandlerType !== "else"){
                    const simpleHandler = (writer, context) =>
                    {
                        if (ifExpr(context))
                            handleMulti(writer, context, ifHandlers);
                    };
                    return [simpleHandler, end];
                }
                const elseIfHandlers = [];
                let elseHandlers = null;
                let elseTokens;
                while(true){
                    [elseTokens, end] = getTokens(template, end + 2);
                    if (elseTokens.length == 1)
                    {
                        [elseHandlers, end] = getBodyHandlers(template, end);
                        break;
                    }
                    else{
                        if (elseTokens[1] !== "if") throw new Error();
                        const elseIfExpr = getExpression(elseTokens, 2);
                        let elseIfInnerHandlers;
                        [elseIfInnerHandlers, end] = getBodyHandlers(template, end);
                        elseIfHandlers.push([elseIfExpr, elseIfInnerHandlers]);
                    }
                    nextHandlerType = getHandlerType(template, end);
                    if (nextHandlerType !== "else")
                        break;
                }
                const handler =  (writer, context) =>{
                    if(ifExpr(context)){
                        handleMulti(writer, context, ifHandlers);
                        return;
                    }
                    for(const [elseIfExpr, elseIfHand] of elseIfHandlers){
                        if (elseIfExpr(context))
                        {
                            handleMulti(writer, context, elseIfHand);
                            return;
                        }
                    }
                    if (elseHandlers)
                    {
                        handleMulti(writer, context, elseHandlers);
                        return;
                    }
                }
                return [handler, end];
            }
            else if (first === "for"){
                const loopVarName = tokens[1];
                if (tokens[2] !== "in") throw new Error();
                const loopValuesExpr = getExpression(tokens, 3);
                let handlers;
                [handlers, end] = getBodyHandlers(template, end);
                const handler =  (writer, context) =>{
                    const loopValues = loopValuesExpr(context);
                    for (const val of loopValues)
                    {
                        context.set(loopVarName, val);
                        handleMulti(writer, context, handlers);
                    }
                }
                return [handler, end];
            }
            else if (first === "end")
            {
                return [null, end];
            }
            else{
                const expr = getExpression(tokens, 0);
                const handler = (writer, context) =>{
                    writer(htmlEncode(expr(context)));
                }
                return [handler, end];
            }
        }
        else if (i < 0)
        {
            return [template.substring(start), template.length];
        }
        else
        {
            return [template.substring(start, i), i];
        }
    }
    function handleMulti(writer, context, handlers){
        for (const h of handlers)
            if (typeof h === "string")
                writer(h);
            else
                h(writer, context);
    }
    function getTokens(template, i){
        const tokens = [];
        while(true){
            while (template[i] === ' ')
                i++;
            if (template[i] === '}' && template[i + 1] === '}')
                return [tokens, i + 2];
            if (template[i] === '.')
            {
                tokens.push(".");
                i++;
                continue;
            }
            const start = i++;
            while (template[i] !== '.' && template[i] !== '}' && template[i] !== ' ')
                i++;
            const token = template.substring(start, i);
            tokens.push(token);
        }
    }
    function getHandlerType(template, start){
        while (start < template.length && ws(template[start]))
            start++;
        if (start + 1 < template.length && template[start] === '{' && template[start + 1] === '{')
        {
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
    function getBodyHandlers(template, end){
        const handlers = [];
        let handler;
        while(true){
            [handler, end] = getHandler(template, end);
            if (handler == null)
                break;
            handlers.push(handler);
        }
        return [handlers, end];
    }
    function getExpression(tokens, start){
        if (tokens.length - start == 1)
        {
            const token = tokens[start];
            return context => context.get(token);
        }
        if (tokens[start + 1] === "."){
            const objName = tokens[start];
            const key = tokens[start + 2];
            return context => {
                return context.get(objName)[key];
            };
        }
        else {
            const f = tokens[start];
            const expr = getExpression(tokens, start + 1);
            return context =>
            {
                var func = context.get(f);
                if (typeof func !== "function")
                    throw new Error(`value of ${f} was not a function`);
                var val = expr(context);
                return func(val);
            };
        }
    }
    function ws(x){
        return x === ' ' || x === '\n' || x === '\t' || x === '\r';
    }
    return { compile };
}();
export default Templater;