/* Main application logic for Hotel Stay UI */
(() => {
  const state = {
    currentResults: [],
    currentSearch: null,
    selectedHotel: null,
    sortDirection: 'asc'
  };

  // Elements
  const searchForm = document.getElementById('search-form');
  const destinationEl = document.getElementById('destination');
  const checkInEl = document.getElementById('check-in');
  const checkOutEl = document.getElementById('check-out');
  const roomTypeEl = document.getElementById('room-type');
  const searchBtn = document.getElementById('search-btn');
  const searchErrors = document.getElementById('search-errors');

  const resultsSection = document.getElementById('results-section');
  const loadingEl = document.getElementById('loading');
  const resultsTable = document.getElementById('results-table');
  const resultsBody = document.getElementById('results-body');
  const resultsCount = document.getElementById('results-count');
  const emptyState = document.getElementById('empty-state');
  const resultsError = document.getElementById('results-error');
  const sortIndicator = document.getElementById('sort-indicator');

  const modalOverlay = document.getElementById('modal-overlay');
  const modalClose = document.getElementById('modal-close');
  const cancelBtn = document.getElementById('cancel-btn');
  const reservationForm = document.getElementById('reservation-form');
  const modalHotelName = document.getElementById('modal-hotel-name');
  const modalRoomType = document.getElementById('modal-room-type');
  const modalDates = document.getElementById('modal-dates');
  const modalTotalPrice = document.getElementById('modal-total-price');
  const reserveBtn = document.getElementById('reserve-btn');
  const reservationErrors = document.getElementById('reservation-errors');

  const confirmOverlay = document.getElementById('confirm-overlay');
  const confirmClose = document.getElementById('confirm-close');
  const confirmationContent = document.getElementById('confirmation-content');
  const copyRefBtn = document.getElementById('copy-ref');
  const viewReservationBtn = document.getElementById('view-reservation');
  const backToSearchBtn = document.getElementById('back-to-search');

  // Aliases for legacy names used in documentation / handlers
  const confirmModal = confirmOverlay;
  const reservationModal = modalOverlay;
  const searchFormSection = document.querySelector('.search-section');

  document.getElementById('sort-asc').addEventListener('click', () => sortResults('asc'));
  document.getElementById('sort-desc').addEventListener('click', () => sortResults('desc'));

  // Search form submit
  searchForm.addEventListener('submit', handleSearchFormSubmit);

  // Delegated click for Reserve buttons
  resultsBody.addEventListener('click', (e) => {
    const btn = e.target.closest('[data-action="reserve"]');
    if (!btn) return;
    const idx = Number(btn.getAttribute('data-index'));
    const hotel = state.currentResults[idx];
    openReservationModal(hotel);
  });

  // Modal handlers
  if (modalClose) modalClose.addEventListener('click', () => closeModal('modal-overlay'));
  if (cancelBtn) cancelBtn.addEventListener('click', () => closeModal('modal-overlay'));
  modalOverlay.addEventListener('click', (e) => { if (e.target === modalOverlay) closeModal('modal-overlay'); });
  document.addEventListener('keydown', (e) => { if (e.key === 'Escape') { closeModal('modal-overlay'); closeModal('confirm-overlay'); } });

  reservationForm.addEventListener('submit', handleReservationSubmit);

  confirmClose.addEventListener('click', closeConfirm);
  copyRefBtn.addEventListener('click', () => {
    const el = confirmationContent.querySelector('[data-ref]');
    if (!el) {
      alert('Reference number not available');
      return;
    }
    const ref = el.getAttribute('data-ref') || el.dataset.ref;
    if (ref) navigator.clipboard?.writeText(ref);
  });

  viewReservationBtn.addEventListener('click', async () => {
    const el = confirmationContent.querySelector('[data-ref]');
    if (!el) { alert('Reference number not available'); return; }
    const ref = el.getAttribute('data-ref') || el.dataset.ref;
    try {
      const data = await window.api.getReservation(ref);
      alert(JSON.stringify(data, null, 2));
    } catch (err) {
      alert(err.message || 'Could not fetch reservation');
    }
  });

  if (backToSearchBtn) {
    backToSearchBtn.addEventListener('click', function() {
      // Close confirmation modal and reservation modal
      closeModal('confirm-overlay');
      closeModal('modal-overlay');

      // Reset application state
      state.currentResults = [];
      state.currentSearch = null;
      state.selectedHotel = null;

      // Reset search form
      const sf = document.getElementById('search-form');
      if (sf) sf.reset();

      // Hide results section
      const rs = document.getElementById('results-section');
      if (rs) rs.classList.add('hidden');

      // Show search form section
      const sfs = document.getElementById('search-form-section');
      if (sfs) sfs.classList.remove('hidden');

      // Clear confirmation content and results body
      if (confirmationContent) confirmationContent.innerHTML = '';
      if (resultsBody) resultsBody.innerHTML = '';
    });
  }

  function setLoading(on) {
    loadingEl.classList.toggle('hidden', !on);
    searchBtn.disabled = on;
  }

  async function handleSearchFormSubmit(e) {
    e.preventDefault();
    searchErrors.textContent = '';
    resultsError.classList.add('hidden');
    emptyState.classList.add('hidden');
    resultsTable.classList.add('hidden');

    const destination = destinationEl.value;
    const checkIn = checkInEl.value;
    const checkOut = checkOutEl.value;
    const roomType = roomTypeEl.value || null;

    const v = window.validator.validateSearchForm(destination, checkIn, checkOut, roomType);
    if (!v.isValid) {
      searchErrors.innerHTML = v.errors.join('<br/>');
      return;
    }

    setLoading(true);
    resultsSection.classList.remove('hidden');
    try {
      const resp = await window.api.searchHotels(destination, checkIn, checkOut, roomType);
      state.currentResults = resp.results || [];
      state.currentSearch = resp;
      if (!state.currentResults.length) {
        emptyState.classList.remove('hidden');
        resultsCount.textContent = '0 results';
      } else {
        displayResults(resp, state.currentResults);
      }
    } catch (err) {
      resultsError.textContent = err.message || 'Search failed';
      resultsError.classList.remove('hidden');
    } finally {
      setLoading(false);
    }
  }

  function displayResults(searchResponse, hotels) {
    resultsBody.innerHTML = '';
    hotels.forEach((h, i) => {
      const tr = document.createElement('tr');
      const badgeClass = h.provider === 'PremierStays' ? 'badge premier' : 'badge budget';
      const amenities = h.amenities ? h.amenities.join(', ') : '';
      const star = h.starRating ? '★'.repeat(Math.max(0, Math.min(5, h.starRating))) : '';

      tr.innerHTML = `
        <td><span class="${badgeClass}">${h.provider}</span></td>
        <td>${escapeHtml(h.hotelName)}</td>
        <td>${h.roomType}</td>
        <td>$${Number(h.pricePerNight).toFixed(2)}</td>
        <td>$${Number(h.totalPrice).toFixed(2)}</td>
        <td>${escapeHtml(formatCancellation(h.cancellationPolicy))}</td>
        <td>${escapeHtml(amenities)}</td>
        <td class="star">${star}</td>
        <td><button data-action="reserve" data-index="${i}" class="btn">Reserve</button></td>
      `;
      resultsBody.appendChild(tr);
    });

    resultsTable.classList.remove('hidden');
    emptyState.classList.add('hidden');
    resultsCount.textContent = `${hotels.length} results`;
  }

  function openReservationModal(hotel) {
    state.selectedHotel = hotel;
    modalHotelName.textContent = hotel.hotelName;
    modalRoomType.textContent = hotel.roomType;
    modalDates.textContent = `${state.currentSearch.checkInDate} → ${state.currentSearch.checkOutDate}`;
    modalTotalPrice.textContent = `$${Number(hotel.totalPrice).toFixed(2)}`;
    reservationErrors.textContent = '';
    reservationForm.reset();
    showModal('modal-overlay');
    document.getElementById('guest-name').focus();
  }

  /**
   * Show a modal with fade animation.
   * Removes the 'hidden' class then adds 'show' to trigger CSS transitions.
   * @param {string} modalId
   */
  function showModal(modalId) {
    const modal = document.getElementById(modalId);
    if (!modal) return;
    modal.classList.remove('hidden');
    // Force reflow then add show class
    requestAnimationFrame(() => modal.classList.add('show'));
  }

  /**
   * Close a modal with fade animation; when transition ends, add hidden.
   * @param {string} modalId
   */
  function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (!modal) return;
    // Remove show to start transition out
    modal.classList.remove('show');
    const onEnd = (e) => {
      // Only handle overlay opacity transition (on overlay element)
      modal.classList.add('hidden');
      modal.removeEventListener('transitionend', onEnd);
    };
    modal.addEventListener('transitionend', onEnd);
    // Fallback: ensure hidden after 300ms if transitionend doesn't fire
    setTimeout(() => { if (!modal.classList.contains('hidden')) modal.classList.add('hidden'); }, 350);
  }

  async function handleReservationSubmit(e) {
    e.preventDefault();
    reservationErrors.textContent = '';
    const guestName = document.getElementById('guest-name').value.trim();
    const documentType = document.getElementById('doc-type').value;
    const documentNumber = document.getElementById('doc-number').value.trim();

    const v = window.validator.validateReservationForm(guestName, documentType, documentNumber);
    if (!v.isValid) {
      reservationErrors.innerHTML = v.errors.join('<br/>');
      return;
    }

    const docCheck = window.validator.validateDocumentForDestination(state.currentSearch.destination, documentType);
    if (!docCheck.isValid) {
      reservationErrors.textContent = docCheck.message;
      return;
    }

    reserveBtn.disabled = true;
    try {
      const conf = await window.api.reserveHotel(
        state.selectedHotel.hotelId,
        state.selectedHotel.provider,
        state.selectedHotel.roomType,
        state.currentSearch.checkInDate,
        state.currentSearch.checkOutDate,
        guestName,
        documentType,
        documentNumber,
        state.currentSearch.destination
      );

      closeModal('modal-overlay');
      displayConfirmation(conf);
    } catch (err) {
      reservationErrors.textContent = err.message || 'Reservation failed';
    } finally {
      reserveBtn.disabled = false;
    }
  }

  function displayConfirmation(conf) {
    const html = `
      <div data-ref="${conf.referenceNumber}">
        <div class="confirm-ref"><h2>${conf.referenceNumber}</h2></div>
        <p><strong>Reserved at:</strong> ${new Date(conf.reservedAt).toLocaleString()}</p>
        <p><strong>Hotel:</strong> ${escapeHtml(conf.hotelName)}</p>
        <p><strong>Provider:</strong> ${escapeHtml(conf.provider)}</p>
        <p><strong>Room:</strong> ${escapeHtml(conf.roomType)}</p>
        <p><strong>Check-in:</strong> ${escapeHtml(conf.checkInDate)} &nbsp; <strong>Check-out:</strong> ${escapeHtml(conf.checkOutDate)}</p>
        <p><strong>Nights:</strong> ${conf.nightsCount}</p>
        <p><strong>Price/Night:</strong> $${Number(conf.pricePerNight).toFixed(2)}</p>
        <p><strong>Total:</strong> $${Number(conf.totalPrice).toFixed(2)}</p>
        <p><strong>Cancellation:</strong> ${escapeHtml(formatCancellation(conf.cancellationPolicy))}</p>
        <p><strong>Guest:</strong> ${escapeHtml(conf.guestName)}</p>
        <p><strong>Document:</strong> ${escapeHtml(conf.documentType)} • ${escapeHtml(conf.documentNumber)}</p>
      </div>
    `;
    confirmationContent.innerHTML = html;
    showModal('confirm-overlay');
  }

  function closeConfirm() { closeModal('confirm-overlay'); }

  function resetToSearch() {
    // clear state
    state.currentResults = [];
    state.currentSearch = null;
    state.selectedHotel = null;
    resultsSection.classList.add('hidden');
    resultsBody.innerHTML = '';
    resultsTable.classList.add('hidden');
  }

  function sortResults(direction) {
    state.sortDirection = direction;
    sortIndicator.textContent = direction === 'asc' ? 'Total Price (Low to High)' : 'Total Price (High to Low)';
    if (!state.currentResults) return;
    state.currentResults.sort((a,b) => direction === 'asc' ? a.totalPrice - b.totalPrice : b.totalPrice - a.totalPrice);
    displayResults(state.currentSearch, state.currentResults);
  }

    function formatCancellation(policy) {
        if (!policy) return '';

        const type = policy.type ?? '';
        const hours = policy.hoursBeforeCheckIn ?? 0;

        if (type === 'NonRefundable') {
            return 'Non-refundable';
        }

        if (type === 'FreeCancellation') {
            return `Free cancellation up to ${hours}h before check-in`;
        }

        if (type === 'Flexible') {
            return `Flexible cancellation up to ${hours}h before check-in`;
        }

        return `${type} up to ${hours}h before check-in`;
    }

  function escapeHtml(unsafe){ return String(unsafe || '').replace(/[&<>"]/g, (c)=>({ '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;' }[c])); }

  // Initialize minimal accessible focus
  destinationEl.focus();
})();
