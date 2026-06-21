const API_BASE = 'http://localhost:5000';

async function handleResponse(res) {
  const contentType = res.headers.get('content-type') || '';
  let body = null;
  if (contentType.includes('application/json')) {
    body = await res.json();
  } else {
    body = await res.text();
  }

  if (!res.ok) {
    const message = (body && body.error) ? body.error : body?.message || `Request failed with status ${res.status}`;
    const err = new Error(message);
    err.status = res.status;
    err.body = body;
    throw err;
  }

  return body;
}

async function searchHotels(destination, checkInDate, checkOutDate, roomType) {
  const params = new URLSearchParams();
  params.append('destination', destination);
  params.append('checkIn', checkInDate);
  params.append('checkOut', checkOutDate);
  if (roomType) params.append('roomType', roomType);

  const url = `${API_BASE}/hotels/search?${params.toString()}`;
  const res = await fetch(url, { method: 'GET' });
  return handleResponse(res);
}

async function reserveHotel(hotelId, provider, roomType, checkInDate, checkOutDate, guestName, documentType, documentNumber, destination) {
  const body = {
    hotelId,
    provider,
    roomType,
    checkInDate,
    checkOutDate,
    guestName,
    documentType,
    documentNumber,
    destination
  };

  const res = await fetch(`${API_BASE}/hotels/reserve`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body)
  });

  return handleResponse(res);
}

async function getReservation(referenceNumber) {
  const res = await fetch(`${API_BASE}/hotels/reservation/${encodeURIComponent(referenceNumber)}`);
  return handleResponse(res);
}

// expose on window for non-module usage
window.api = {
  searchHotels,
  reserveHotel,
  getReservation,
  API_BASE
};
