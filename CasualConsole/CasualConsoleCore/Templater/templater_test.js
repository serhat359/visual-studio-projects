import Templater from "./templater.js";

var tests = [
    ["", { }, ""],
    ["hello", { }, "hello"],
    ["<script>", { }, "<script>"],
    ["hello {{w}}", { w:"world" }, "hello world"],
    ["hello {{w}}", { w:"<script>" }, "hello &lt;script&gt;"],
    ["hello {{upper w}}", { w:"world" }, "hello WORLD"],
    ["{{h}} {{w}}", { h:"hello", w:"world" }, "hello world"],
    ["the number is {{if b}}there{{end}}{{else}}NOT there{{end}}", { b:2 }, "the number is there"],
    ["the number is {{if b}}there{{end}}{{else}}NOT there{{end}}", { b:0 }, "the number is NOT there"],
    ["numbers: {{for x in num.inner}}{{x}},{{end}}", { num: { inner: [1,2,3] } }, "numbers: 1,2,3,"],
    ["numbers count: {{num.length}}", { num: [1,2,3] }, "numbers count: 3"],
    ["numbers count: {{text.length}}", { text: "hello world" }, "numbers count: 11"],
    ["num: {{numF}}", { get numF(){ return 3; } }, "num: 3"],
    ["{{a.b}}", { a: { b: "text" } }, "text"],
    ["{{for e in num}}{{e.x}}{{end}}", { num: [{x:2},{x:5},{x:8}] }, "258"],
    ["{{if arr.length}}{{for a in arr}}{{end}}{{end}}{{else}}No record found{{end}}", { arr:[] }, "No record found"],
    ["{{fixed number}}", { number:2.762736723 }, "2.76"],
    ["{{fixed a.b}}", { a: { b:2.762736723 } }, "2.76"],
    ["{{if isPos a.b}}YES{{end}}{{else}}NO{{end}}",  { a: { b:2.762736723 } }, "YES"],
    ["{{if x}}{{end}}{{x}}", { x:2 }, "2"],
    ["{{if x}}{{end}}    {{else if x}}   {{end}}    {{x}}", { x:2 }, "    2"],
    ["{{for x in texts}}<{{x}}>{{end}}", { texts: ["foo", "bar", "baz", "<script>"] }, "<foo><bar><baz><&lt;script&gt;>"],
];

let helpers = {
    fixed(x){
        return x.toFixed(2);
    },
    upper(s){
        return s.toUpperCase();
    },
    isPos(d){
        return d > 0;
    }
};

for (let [template, data, expected] of tests){
    let compiled = Templater.compile(template);
    let rendered = compiled(data, helpers);
    if (rendered !== expected)
        throw new Error();
}

console.log("SUCCESS!!");
