document.addEventListener('DOMContentLoaded', () => {
    let cartButtons = document.querySelectorAll('[data-cart-product]');
    for (let btn of cartButtons) {
        btn.addEventListener('click', addCartClick);
    }

    for (let btn of document.querySelectorAll('[data-cart-detail-dec]')) {
        btn.addEventListener('click', decCartClick);
    }
    for (let btn of document.querySelectorAll('[data-cart-detail-inc]')) {
        btn.addEventListener('click', incCartClick);
    }
    for (let btn of document.querySelectorAll('[data-cart-detail-del]')) {
        btn.addEventListener('click', delCartClick);
    }
    for (let btn of document.querySelectorAll('[data-cart-detail-cnt]')) {
        btn.addEventListener('keydown', editCartEdit);
        btn.addEventListener('blur', editCartBlur);
        btn.addEventListener('focus', editCartFocus);
    }
    for (let btn of document.querySelectorAll('[data-cart-cancel]')) {
        btn.addEventListener('click', cancelCart);
    }
    for (let btn of document.querySelectorAll('[data-cart-buy]')) {
        btn.addEventListener('click', buyCart);
    }

    for (let btn of document.querySelectorAll('[rate-button]')) {
        btn.addEventListener('click', rateClick);
    }
});

function rateClick(e) {
    e.preventDefault();

    const btn = e.target.closest('[data-rate-user]');
    const userId = btn.getAttribute('data-rate-user');
    const productId = btn.getAttribute('data-rate-product');
    const commentInput = document.getElementById("rate-comment");
    const ratingInput = document.querySelector('input[name="Rate"]:checked');

    const rating = ratingInput ? ratingInput.value : null;
    const comment = commentInput.value.trim();

    document.querySelectorAll(".is-invalid").forEach(el => el.classList.remove("is-invalid"));
    document.querySelectorAll(".invalid-feedback").innerText = "";

    fetch("/Shop/Rate", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userId, productId, comment, rating })
    })
        .then(r => r.json())
        .then(j => {
            if (j.status >= 400) {
                if (j.errors.comment) {
                    showError(commentInput, j.errors.comment);
                }
                if (j.errors.rating) {
                    showError(document.querySelector(".star-rate"), j.errors.rating);
                }
                return;
            }
            window.location.reload();
        });
}


function showError(input, message) {
    input.classList.add("is-invalid");
    const rateFeedback = input.parentNode.querySelector("#rate-feedback");
    rateFeedback.innerText = message;
}

function buyCart(e) {
    const idElement = e.target.closest("[data-cart-buy]");
    if (!idElement) throw "buyCart() error: [data-cart-buy] not found";

    const cartId = idElement.getAttribute("data-cart-buy");
    if (!cartId) throw "buyCart() error: [data-cart-buy] attribute empty or not found";

    console.log(cartId);

    fetch("/Shop/CloseCart/" + cartId, {
        method: 'DELETE',
        headers: {
            'Cart-Action': 'Buy',
        }
    }).then(r => r.json()).then(j => {
        if (j.status < 300) {
            alert("Придбано");
            window.location.reload();
        }
        else {
            alert("Якась халепа. " + j.message);
        }
    });
}

function cancelCart(e) {
    const idElement = e.target.closest("[data-cart-cancel]");
    if (!idElement) throw "cancelCart() error: [data-cart-cancel] not found";

    const cartId = idElement.getAttribute("data-cart-cancel");
    if (!cartId) throw "cancelCart() error: [data-cart-cancel] attribute empty or not found";

    console.log(cartId);

    fetch("/Shop/CloseCart/" + cartId, {
        method: 'DELETE'
    }).then(r => r.json()).then(j => {
        if (j.status < 300) {
            alert("Скасовано");
            window.location.reload();
        }
        else {
            alert("Якась халепа. " + j.message);
        }
    });
}

/*
Додати діалоги погодження операцій роботи з елементами кошику:
видалення - "Ви видаляєте позицію 'Кіт' з кошику. Підтверджуєте? " [так][ні]
ручне введення кількості - "Ви змінюєте кількість замовлення 'Кіт' з 2 до 10 шт. Підтверджуєте? " [так][ні]
** реалізувати діалогами Bootstrap
*/
function editCartFocus(e) {
    e.target.beforeEditing = e.target.innerText;
}
function editCartBlur(e) {
    if (e.target.innerText === "") e.target.innerText = e.target.beforeEditing;

    if (e.target.beforeEditing != e.target.innerText) {
        const delta = Number(e.target.innerText) - Number(e.target.beforeEditing);
        const cdElement = e.target.closest("[data-cart-detail-cnt]");
        const cdId = cdElement.getAttribute("data-cart-detail-cnt");

        console.log(`Changes: ${e.target.beforeEditing} -> ${e.target.innerText} d=${delta} id=${cdId}`);

        fetch(`/Shop/ModifyCart/${cdId}?delta=${delta}`, {
            method: 'PATCH'
        }).then(r => r.json())
            .then(j => {
                console.log(j);
                if (j.status < 300) {
                    window.location.reload();
                }
                else {
                    alert("Помилка: " + j.message);
                    e.target.innerText = e.target.beforeEditing;
                }
            });
    }
}
function editCartEdit(e) {
    if (![8, 13, 37, 39, 46, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57].includes(e.keyCode)) {
        e.preventDefault();
        return true;
    }
    if (e.keyCode == 13) {
        e.target.blur();
    }
}

function delCartClick(e) {
    e.stopPropagation();
    const cdElement = e.target.closest("[data-cart-detail-del]");
    const cdId = cdElement.getAttribute("data-cart-detail-del");
    const spanElement = cdElement.parentNode.querySelector('[data-cart-detail-cnt]');
    const delta = -Number(spanElement.innerText);
    fetch(`/Shop/ModifyCart/${cdId}?delta=${delta}`, {
        method: 'PATCH'
    }).then(r => r.json())
        .then(j => {
            console.log(j);
            if (j.status < 300) {
                window.location.reload();
            }
            else {
                alert("Помилка: " + j.message);
            }
        });
}

function decCartClick(e) {
    e.stopPropagation();
    const cdElement = e.target.closest("[data-cart-detail-dec]");
    const cdId = cdElement.getAttribute("data-cart-detail-dec");
    console.log("- " + cdId);

    fetch(`/Shop/ModifyCart/${cdId}?delta=-1`, {
        method: 'PATCH'
    }).then(r => r.json())
        .then(j => {
            console.log(j);
            if (j.status < 300) {
                location = location;
            }
            else {
                alert("Помилка: " + j.message);
            }
        });
}

function incCartClick(e) {
    e.stopPropagation();
    const cdElement = e.target.closest("[data-cart-detail-inc]");
    const cdId = cdElement.getAttribute("data-cart-detail-inc");
    console.log("+ " + cdId);

    fetch(`/Shop/ModifyCart/${cdId}?delta=1`, {
        method: 'PATCH'
    }).then(r => r.json())
        .then(j => {
            console.log(j);
            if (j.status < 300) {
                window.location.reload();
            }
            else {
                alert("Помилка: " + j.message);
            }
        });

}


function addCartClick(e) {
    e.stopPropagation();
    const cartElement = e.target.closest('[data-cart-product]');
    const productId = cartElement.getAttribute('data-cart-product');
    console.log(productId);

    fetch('/Shop/AddToCart/' + productId, {
        method: 'PUT',
    })
        .then(r => r.json())
        .then(j => {
            console.log(j);
            if (j.status == 401) {
                openModal('Error', 'Please log in to place an order.');
                return;
            }
            else if (j.status == 400) {
                openModal('Error', 'Invalid product ID format. Please try again.');
                return;
            }
            else if (j.status == 404) {
                openModal('Error', 'The selected product was not found. It may no longer be available.');
                return;
            }
            else if (j.status == 201) {
                openModal('Success', 'The product has been added. Would you like to go to your cart?', true);
                return;
            }
            else {
                openModal('Error', 'Something went wrong!');
                return;
            }
        })
        .catch(error => {
            console.error('Fetch error:', error);
            openModal('Error', 'Failed to add product. Please try again later.');
        });
}

function openModal(title, message, success = false) {
    const confirmButton = success ? `<button type="button" class="btn btn-primary" id="cart-btn" data-bs-dismiss="modal">Перейти до кошику</button>` : '';
    const modalHTML = `<div class="modal" id="cartModal" tabindex=" - 1">
                     <div class="modal-dialog">
                         <div class="modal-content">
                             <div class="modal-header">
                                 <h5 class="modal-title">${title}</h5>
                                 <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                             </div>
                             <div class="modal-body">
                                 <p>${message}</p>
                             </div>
                             <div class="modal-footer">
                                 <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Закрити</button>
                                 ${confirmButton}
                             </div>
                         </div>
                     </div>
                   </div>`;
    document.body.insertAdjacentHTML('beforeend', modalHTML);
    const modalWindow = new bootstrap.Modal(document.getElementById('cartModal'));
    modalWindow.show();
    if (success) {
        document.getElementById('cart-btn').addEventListener('click', function () {
            window.location = '/User/Cart';
        });
    }
    document.getElementById('cartModal').addEventListener('hidden.bs.modal', function () {
        document.getElementById('cartModal').remove();
    });
};


document.addEventListener('submit', e => {
    const form = e.target;
    if (form.id === "auth-form") {
        console.log("here")

        e.preventDefault();

        const loginInput = form.querySelector('[name="UserLogin"]');
        const passwordInput = form.querySelector('[name="Password"]');
        const errorContainer = document.getElementById('auth-errors');

        let errors = []; 

 
        errorContainer.textContent = "";

        if (loginInput.value.trim() === "") {
            errors.push("Логін не може бути порожнім.");
        }

        const password = passwordInput.value;
        if (password.length < 8) {
            errors.push("Пароль повинен бути не менше 8 символів.");
        }
        if (!/[A-Za-z]/.test(password)) {
            errors.push("Пароль повинен містити хоча б одну літеру.");
        }
        if (!/\d/.test(password)) {
            errors.push("Пароль повинен містити хоча б одну цифру.");
        }
        if (!/[\W_]/.test(password)) {
            errors.push("Пароль повинен містити хоча б один спеціальний символ.");
        }


        if (errors.length > 0) {
            errorContainer.innerHTML = errors.join("<br>");
           
        }

        // Генерація `credentials` для авторизації

        // RFC 7617
        const credentials = btoa(loginInput.value + ':' + passwordInput.value);
        fetch("/User/Authenticate", {
            method: "GET",
            headers: {
                'Authorization': 'Basic ' + credentials
            }
        }).then(r => {
            console.log(r);
            if (r.ok) {
                window.location.reload();
            }
            else {
                r.json().then(j => {
                    alert(j);   // Д.З.
                });
            }
        });

        console.log(credentials);   
    }


});

const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))