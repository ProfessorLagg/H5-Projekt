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
    return result;
}

/**
 * Converts this DOMRect from viewport coordinates to UV coordinates
 * @returns A new DOMRect in UV coordinates
 */
DOMRect.prototype.uv = function () {
    return new DOMRect(
        this.x / window.visualViewport.width,
        this.y / window.visualViewport.height,
        this.width / window.visualViewport.width,
        this.height / window.visualViewport.height
    );
}

/**
 * finds the center of this DOMRect
 * @returns A 0 width, 0 height DOMRect with x/y set to the center of this DOMRect
 */
DOMRect.prototype.center = function () {
    return new DOMRect(
        this.x + (this.width / 2),
        this.y + (this.height / 2),
        0,
        0
    );
}