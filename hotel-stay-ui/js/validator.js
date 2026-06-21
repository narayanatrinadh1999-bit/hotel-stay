// Validator utilities for client-side checks
const INTERNATIONAL = ['London','Paris','Tokyo'];
const DOMESTIC = ['New Delhi','Hyderabad'];

function validateSearchForm(destination, checkInDate, checkOutDate, roomType) {
  const errors = [];
  if (!destination || ![...INTERNATIONAL,...DOMESTIC].includes(destination)) {
    errors.push('Destination is required and must be one of New Delhi, Hyderabad, London, Paris, Tokyo.');
  }

  if (!checkInDate || isNaN(Date.parse(checkInDate))) {
    errors.push('Check-in date is required and must be a valid date.');
  }

  if (!checkOutDate || isNaN(Date.parse(checkOutDate))) {
    errors.push('Check-out date is required and must be a valid date.');
  }

  if (checkInDate && checkOutDate && Date.parse(checkOutDate) <= Date.parse(checkInDate)) {
    errors.push('Check-out date must be after check-in date.');
  }

  if (roomType && roomType !== '' && !['Standard','Deluxe','Suite'].includes(roomType)) {
    errors.push('Invalid room type.');
  }

  return { isValid: errors.length === 0, errors };
}

function validateReservationForm(guestName, documentType, documentNumber) {
  const errors = [];
  if (!guestName || guestName.trim() === '') errors.push('Guest name is required.');
  if (!documentType || !['Passport','NationalId'].includes(documentType)) errors.push('Document type is required.');
  if (!documentNumber || documentNumber.trim() === '') errors.push('Document number is required.');
  return { isValid: errors.length === 0, errors };
}

function validateDocumentForDestination(destination, documentType) {
  if (INTERNATIONAL.includes(destination)) {
    if (documentType === 'Passport') return { isValid: true, message: '' };
    return { isValid: false, message: `${destination} is an international destination. Passport is required.` };
  }
  if (DOMESTIC.includes(destination)) return { isValid: true, message: '' };
  return { isValid: false, message: `Destination '${destination}' not recognized.` };
}

function getInternationalDestinations(){ return INTERNATIONAL.slice(); }
function getDomesticDestinations(){ return DOMESTIC.slice(); }

window.validator = {
  validateSearchForm,
  validateReservationForm,
  validateDocumentForDestination,
  getInternationalDestinations,
  getDomesticDestinations
};
