/**
 * Tests if part of this DOMRect intersects any part of another DOMRect
 * @param {DOMRect} other 
 * @returns true if any part of this DOMRect intersects any part of the other DOMRect
 */
DOMRect.prototype.intersects = function (other) {
    
    if (!(other instanceof DOMRect)) { return false; }

    const testT = this.top >= other.top && this.top <= other.bottom
    const testL = this.left >= other.left && this.left <= other.right;

    const result = testT && testL;
    console.debug(arguments.callee.name, 
        "\n\tthis:", this,
        "\n\tother:", other,
        "\n\ttestT:", testT,
        "\n\ttestL:", testL,
        "\n\tresult:", result,
    )
    return result;
}