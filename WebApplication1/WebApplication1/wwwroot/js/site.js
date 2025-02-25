document.addEventListener('DOMContentLoaded', () => {
    let cartButtons = document.querySelectorAll('[data-cart-product]');
    for (let btn of cartButtons) {
        btn.addEventListener('click', addCartClick);
    }
});

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