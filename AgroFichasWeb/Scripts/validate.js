function FormatNumber(number) 
{
    var n = number + ''; //cast to string
    var s = '';
    var j = -1;
    for (var i = n.length - 1; i >= 0; i--) {
        j++;
        if (j > 0 && j % 3 == 0)
            s = ',' + s;
        s = n[i] + s;
    }

    return s;
}

function EsRutValido(strRut)
{
    if (!EsFormatoRut(strRut)) {
        alert('El Formato del Rut no es válido.');
        return false;
    }
    var s = strRut;

    //sacamos lala raya
    s = s.replace(/\-/g, '');
    
    var t  = s.substring(0, s.length - 1);
    var dv = s.charAt(s.length-1);

    /* t: Numero del Rut, dv: Dígito verificador */
    var total = 0;
    var factor = 2;
    for (i = t.length - 1; i >= 0; i--) {
        total += parseInt(t.charAt(i), 10) * factor;
        if (factor == 7) {
            factor = 2;
        }
        else {
            factor += 1;
        }
    }

    var dvd = 11 - (total % 11);
    switch(dvd)
    {
        case 10: 
            dvd = 'K';
            break;
        case 11:
            dvd = '0';
            break;
        default:
            dvd = new String(dvd);
    }
    
    if (dvd.toUpperCase() == dv.toUpperCase()) {
        return true;
    }
    else {
        alert('El Rut no es válido.\n\nDV Ingresado = ' + dv + '\n\nDV Calculado = ' + dvd);
        return false;
    }
}


function EsFormatoRut(strValor)
{
	var objRegExp = /^\d+\-([0-9]|K|k)$/ 
 		//check to see if in correct format  
	if(!objRegExp.test(strValor)) {
   		return false; //doesn't match pattern, bad date  
	}
	return true;

}


function jqEsTextoFecha(objTexto, bolPermitirNulo) {
    objTexto.val(Trim(objTexto.val()));
    if (bolPermitirNulo && objTexto.val().length == 0) {
        return true;
    }
    if (EsFecha(objTexto.val())) {
        return true;
    }
    else {
        return false;
    }
}

function EsTextoFecha(objTexto, bolPermitirNulo)
{
	objTexto.value = Trim(objTexto.value);
	if (bolPermitirNulo && objTexto.value.length == 0) {
		return true;
	}
	if (EsFecha(objTexto.value)) {
		return true;
	}
	else {
		return false;
	}
}


function EsFecha(strValor)
{
    var objRegExp = /^\d{2}\/\d{2}\/\d{4}$/
    var objRegExp2 = /^\d{2}\-\d{2}\-\d{4}$/ 
  	//check to see if in correct format  
	if(!objRegExp.test(strValor) && !objRegExp2.test(strValor)) {
    	return false; //doesn't match pattern, bad date  
	}
	else{
	    var strSeparator = strValor.substring(2, 3) //find date separator
    	var arrayDate = strValor.split(strSeparator); //split date into month, day, year
    	//create a lookup for months not equal to Feb.
    	var arrayLookup = { '01' : 31,'03' : 31, '04' : 30,'05' : 31,'06' : 30,'07' : 31,
                        	'08' : 31,'09' : 30,'10' : 31,'11' : 30,'12' : 31}
		var intDay = parseInt(arrayDate[0], 10); 
    	//check if month value and day value agree
    	if(arrayLookup[arrayDate[1]] != null) {
      		if(intDay <= arrayLookup[arrayDate[1]] && intDay != 0)
        		return true; //found in lookup table, good date    
		}    
    	//check for February    
		var intYear = parseInt(arrayDate[2],10);
    	var intMonth = parseInt(arrayDate[1],10);
    	if( ((intYear % 4 == 0 && intDay <= 29) || (intYear % 4 != 0 && intDay <=28)) && intDay !=0)
      		return true; //Feb. had valid number of days  
    }
  	return false; //any other values, bad date
}

function EsTextoYM(objTexto, bolPermitirNulo)
{
	objTexto.value = Trim(objTexto.value);
	if (bolPermitirNulo && objTexto.value.length == 0) {
		return true;
	}
	if (EsYM(objTexto.value)) {
		return true;
	}
	else {
		return false;
	}
}


function EsYM(strValor)
{
 	var objRegExp = /^\d{2}\/\d{4}$/ 
  	//check to see if in correct format  
	if(!objRegExp.test(strValor)) {
    	return false; //doesn't match pattern, bad date  
	}
	else{
    	//var strSeparator = strValor.substring(2,3) //find date separator
    	var arrayDate = strValor.split('/'); //split date into month, day, year
    	//create a lookup for months not equal to Feb.
    	var arrayLookup = { '01' : 31,'03' : 31, '04' : 30,'05' : 31,'06' : 30,'07' : 31,
                        	'08' : 31,'09' : 30,'10' : 31,'11' : 30,'12' : 31}
		//var intDay = parseInt(arrayDate[0], 10); 
    	//check if month value and day value agree
    	if(arrayLookup[arrayDate[0]] != null) {
        		return true; //found in lookup table, good date    
		}    
    	//check for February    
    }
  	return false; //any other values, bad date
}


function jqEsTextoEntero(objTexto, bolPermitirNulo, intMinimo, intMaximo) {
    objTexto.val(Trim(objTexto.val()));
    if (bolPermitirNulo && objTexto.val().length == 0) {
        return true;
    }
    if (EsEntero(objTexto.val(), intMinimo, intMaximo)) {
        objTexto.val(parseInt(objTexto.val(), 10));
        return true;
    }
    else {
        return false;
    }
}

function jqTrimTxt(objTexto) {
    objTexto.val(Trim(objTexto.val()));
    return;
}

function EsTextoEntero(objTexto, bolPermitirNulo, intMinimo, intMaximo)
{
	objTexto.value = Trim(objTexto.value);
	if (bolPermitirNulo && objTexto.value.length == 0) {
		return true;
	}
	if (EsEntero(objTexto.value, intMinimo, intMaximo)) {
		objTexto.value = parseInt(objTexto.value,10);
		return true;
	}
	else {
		return false;
	}
}

function jqEsTextoNumeric(objTexto, bolPermitirNulo, intMinimo, intMaximo) {
    objTexto.val(Trim(objTexto.val()));
    objTexto.val(objTexto.val().replace(/,/g, "."));
    if (bolPermitirNulo && objTexto.val().length == 0) {
        return true;
    }
    if (EsNumeric(objTexto.val(), intMinimo, intMaximo)) {
        objTexto.val(parseFloat(objTexto.val()));
        return true;
    }
    else {
        return false;
    }
}

function EsTextoNumeric(objTexto, bolPermitirNulo, intMinimo, intMaximo)
{
	objTexto.value = Trim(objTexto.value);
	objTexto.value = objTexto.value.replace(/,/g,".");
	if (bolPermitirNulo && objTexto.value.length == 0) {
		return true;
	}
	if (EsNumeric(objTexto.value, intMinimo, intMaximo)) {
		objTexto.value = parseFloat(objTexto.value);
		return true;
	}
	else {
		return false;
	}
}


function EsEntero(strValor, intMinimo, intMaximo)
{
	var objRegExp = /(^(\+|-)?\d\d*$)/;
	var intValor;
	
	if (objRegExp.test(strValor)) {
		intValor = parseInt(strValor,10);
		if (! isNaN(intMinimo)) {
			if (intValor < intMinimo)
				return false
		}
		if (! isNaN(intMaximo)) {
			if (intValor > intMaximo)
				return false;
		}
		return true;
	}
	else {
		return false;
	}
}

function EsNumeric(strValor, numMinimo, numMaximo)
{
	var objRegExp = /(^(\+|-)?\d\d*\.\d*$)|(^(\+|-)?\d\d*$)/; 
	var numValor;
	
	if (objRegExp.test(strValor)) {
		numValor = parseFloat(strValor);
		if (! isNaN(numMinimo)) {
			if (numValor < numMinimo)
				return false
		}
		if (! isNaN(numMaximo)) {
			if (numValor > numMaximo)
				return false;
		}
		return true;
	}
	else {
		return false;
	}
}

function Trim(strValue) 
{
	/************************************************
	DESCRIPTION: Removes leading and trailing spaces.
	PARAMETERS: Source string from which spaces will  be removed;
	RETURNS: Source string with whitespaces removed.
	*************************************************/  
	var objRegExp = /^(\s*)$/;
    //check for all spaces    
	if(objRegExp.test(strValue)) {
       strValue = strValue.replace(objRegExp, '');
       if( strValue.length == 0)          
	   		return strValue;    
	}    
   //check for leading & trailing spaces
   objRegExp = /^(\s*)([\W\w]*)(\b\s*$)/;   
   if(objRegExp.test(strValue)) {
       //remove leading and trailing whitespace characters
       strValue = strValue.replace(objRegExp, '$2');
	}  
	return strValue;
}

function LPad(Texto, Largo)
{
    var s = Texto;
    
    if (s.length > Largo) {
       s = s.substring(0, Largo-1)+ '.';
    }
    else if(s.length == Largo) {
        s = s;
    }
    else {
        while (s.length < Largo)
        {
            s = ' ' + s;
        }
    }
    return s;
}

function RPad(Texto, Largo)
{
    var s = Texto;
    
    if (s.length > Largo) {
       s = s.substring(0, Largo);
    }
    else if(s.length == Largo) {
        s = s;
    }
    else {
        while (s.length < Largo)
        {
            s = s + ' ' ;
        }
    }
    return s;
}

function TrimTxt(objTexto)
{
    objTexto.value = Trim(objTexto.value);
}

function StrToFecha(Texto)
{
    var aux = Texto;
    //31/12/2019
    //0123456789
	return new Date(aux.substr(6,4), parseInt(aux.substr(3, 2), 10) - 1, aux.substr(0, 2));
}

function JsonToFecha(json) 
{
    return eval("new " + json.slice(1, -1));
}

function JsonToTimestamp(json) 
{
    var d = JsonToFecha(json);
    return d.getFullYear() + "-" + d.getDate() + "-" + (d.getMonth() + 1);
}

function FechaToTimestamp(fecha) 
{
    return fecha.getFullYear() + "-" + fecha.getDate() + "-" + (fecha.getMonth() + 1);
}

function PromptForRut(OnSuccessGoTo) 
{
    var s = prompt("Ingrese el Rut:");
    if (!s || s == "" || !EsRutValido(s))
        return;

    document.location(OnSuccessGoTo + '?key=' + s);
}


function showValidationErrors(msg, errorList, element) {

    var s = '';
    if (msg != null && msg != '')
        s += '<p>' + msg + '</p>';

    if (errorList != null && errorList.length > 0) {

        s += "<ul>";

        for (i = 0; i < errorList.length; i++) {
            s += '<li>' + errorList[i].ErrorMessage + '</li>';
        }

        s += "</ul>";
    }

    if (s != '') {
        $(element).html(s);
        $(element).show();
    }
    else {
        $(element).hide();
    }
}

function alertValidationErrors(errorList) {

    var s = 'Corrige lo siguiente: \n\n';

    if (errorList != null && errorList.length > 0) {

        for (i = 0; i < errorList.length; i++) {
            s += '- ' + errorList[i] + '\n';
        }
    }

    alert(s);
}
function parsePostFail(jqXHR, exception) {
    // Our error logic here
    var msg = '';
    if (jqXHR.status === 0) {
        msg = 'No hay conexión de red.';
    } else if (jqXHR.status == 404) {
        msg = 'No se encontró la página. [404]';
    } else if (jqXHR.status == 500) {
        msg = 'Error interno del servidor [500].';
    } else if (exception === 'parsererror') {
        msg = 'Requested JSON parse failed.';
    } else if (exception === 'timeout') {
        msg = 'Time out error.';
    } else if (exception === 'abort') {
        msg = 'Ajax request aborted.';
    } else {
        msg = 'Error desconocido. ' + jqXHR.responseText;
    }

    return msg;
}