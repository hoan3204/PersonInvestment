(function () {
    function submitPostForm(action, fields) {
        const form = document.createElement('form');
        form.method = 'post';
        form.action = action;

        Object.entries(fields).forEach(([name, value]) => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = name;
            input.value = value;
            form.appendChild(input);
        });

        document.body.appendChild(form);
        form.submit();
    }

    function askQuantity(defaultValue) {
        const quantityText = window.prompt('Nhập số lượng:', defaultValue);

        if (quantityText === null) {
            return null;
        }

        const quantity = parseInt(quantityText, 10);
        if (Number.isNaN(quantity) || quantity <= 0) {
            window.alert('Số lượng không hợp lệ. Vui lòng nhập số lớn hơn 0.');
            return null;
        }

        return quantity;
    }

    window.buyProduct = function (productId) {
        const quantity = askQuantity(1);
        if (quantity === null) return;

        submitPostForm('/Transaction/Buy', { productId, quantity });
    };

    window.sellProduct = function (productId) {
        const quantity = askQuantity(1);
        if (quantity === null) return;

        submitPostForm('/Transaction/Sell', { productId, quantity });
    };

    window.toggleWishlist = function (productId) {
        submitPostForm('/WatchList/Add', { productId });
    };
})();