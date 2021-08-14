function keyToDirection(e){
    if(e.altKey || e.ctrlKey || e.metaKey || e.shiftKey){
        return "";
    }
    if(e.code=="ArrowUp" || e.code=="ArrowLeft" || e.code=="PageUp" || e.code=="KeyK"){
        return "prev();";
    }
    if(e.code=="ArrowDown" || e.code=="ArrowRight" || e.code=="PageDown" || e.code=="KeyJ"){
        return "next();";
    }
}