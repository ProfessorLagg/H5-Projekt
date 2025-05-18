export { GameElement, syncGet }
// Imports https://html.spec.whatwg.org/multipage/webappapis.html#module-type-allowed
import * as prng from "./prng.mjs";
import game_css from "./game.css" with { type: "css" };
import shapes from "./shapes.json" with { type: "json" };

// -- utils --
function syncGet(url) {
    let httpRequest = new XMLHttpRequest();
    httpRequest.open("GET", url, false);
    httpRequest.send();
    return httpRequest.response;
}

// -- game template --
const template_url = import.meta.resolve("./game.html");
function loadTemplate() {
    console.debug("loadTemplate()");
    let txt = syncGet(template_url);
    let elem = new DOMParser().parseFromString(txt, "text/html").head.firstChild;
    elem.id = "tmpl-" + GameElementTagName;
    return elem;
}

// -- GameElement --
const GameElementTagName = 'game-wrap';
class GameElement extends HTMLElement {
    static indexTo1D(row, col) {
        const r = Math.max(0, Math.min(9, row));
        const c = Math.max(0, Math.min(9, col));
        return r * 9 + c;
    }
    static indexTo2D(index) {
        const idx = Math.max(0, Math.min(80, index));
        const col = idx % 9;
        const row = (idx - col) / 9
        return {
            row: row,
            col: col,
        }
    }

    restart() {
        console.debug(this.localName, "restart()");
        this.seed = prng.sfc32.getSeed();
        this.rand = new prng.sfc32(this.seed);
    }

    connectedCallback() {
        console.debug(this.localName, "connectedCallback()");

        let shadowRoot = this.attachShadow({ mode: "open" });
        shadowRoot.adoptedStyleSheets = [game_css];

        shadowRoot.appendChild(this.template.content.cloneNode(true));

        this.restart();
    }
    
    constructor() {
        super();
        this.template = loadTemplate();
        this.seed = prng.sfc32.getSeed();
        this.rand = new prng.sfc32(this.seed);
    }
}

customElements.define(GameElementTagName, GameElement);
